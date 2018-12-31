using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Dendrite
{

    public class SkinnedDendrite : DendriteBase {

        public override DendriteType Type { get { return DendriteType.Skinned; } }
        public override Bounds Bounds { get { return skinnedRenderer.sharedMesh.bounds; } }

        [SerializeField] protected TextAsset pointsAsset;
        [SerializeField] protected SkinnedMeshRenderer skinnedRenderer;

        SkinnedAttraction[] attractions;

        protected ComputeBuffer bindPoseBuffer;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void Start() {
            base.Start();

            var bindposes = skinnedRenderer.sharedMesh.bindposes;
            bindPoseBuffer = new ComputeBuffer(bindposes.Length, Marshal.SizeOf(typeof(Matrix4x4)));
            bindPoseBuffer.SetData(bindposes);

            attractions = GeneratePoints(pointsAsset);
            attractionBuffer = new ComputeBuffer(attractions.Length, Marshal.SizeOf(typeof(SkinnedAttraction)), ComputeBufferType.Default);
            attractionBuffer.SetData(attractions);

            Reset();
        }
        
        protected override void Update () {
            base.Update();

            Animate(Time.timeSinceLevelLoad, Time.deltaTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            bindPoseBuffer.Dispose();
        }

        protected void OnDrawGizmos()
        {
            // DrawGizmos();
        }

        protected void DrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            var mesh = skinnedRenderer.sharedMesh;
            var vertices = mesh.vertices;
            for(int i = 0, n = vertices.Length; i < n; i++)
            {
                var v = vertices[i];
                Gizmos.DrawSphere(v, 0.001f);
            }

#if UNITY_EDITOR
            if (attractionBuffer == null) return;
            UnityEditor.Handles.matrix = transform.localToWorldMatrix;

            var tmp = new SkinnedAttraction[attractionBuffer.count];
            attractionBuffer.GetData(tmp);
            for(int i = 0, n = tmp.Length; i < n; i++)
            {
                var attr = tmp[i];
                UnityEditor.Handles.Label(attr.position, attr.bone.ToString());
            }
#endif

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

            var bindposes = skinnedRenderer.sharedMesh.bindposes;
            var bones = skinnedRenderer.bones.Select(bone => bone.localToWorldMatrix).ToArray();

            var nodes = new SkinnedNode[nodeBuffer.count];
            nodeBuffer.GetData(nodes);
            for (int i = 0, n = nodes.Length; i < n; i++)
            {
                var node = nodes[i];
                if (node.active <= 0) continue;
                var bind = bindposes[node.index0];
                var bone = bones[node.index0];
                Vector3 p = (bone * bind).MultiplyPoint(node.position);
                p = transform.TransformPoint(p);
                Gizmos.DrawSphere(p, 0.001f);
            }


        }

        #endregion

        protected void SetupSkin()
        {
            var mesh = skinnedRenderer.sharedMesh;
            var vertices = mesh.vertices;
            var weights = mesh.boneWeights;
            var indices = new int[weights.Length];
            for(int i = 0, n = weights.Length; i < n; i++)
                indices[i] = weights[i].boneIndex0;

            using (
                ComputeBuffer
                vertBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(Vector3))),
                boneBuffer = new ComputeBuffer(weights.Length, Marshal.SizeOf(typeof(uint)))
            )
            {
                vertBuffer.SetData(vertices);
                boneBuffer.SetData(indices);

                var kernel = compute.FindKernel("SetupSkin");
                compute.SetBuffer(kernel, "_Vertices", vertBuffer);
                compute.SetBuffer(kernel, "_Bones", boneBuffer);
                compute.SetBuffer(kernel, "_Attractions", attractionBuffer);
                GPUHelper.Dispatch1D(compute, kernel, attractionBuffer.count);
            }

        }

        public override void Reset()
        {
            base.Reset();

            count = attractions.Length * 2;

            SetupSkin();

            nodeBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(SkinnedNode)), ComputeBufferType.Default);
            nodePoolBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
            nodePoolBuffer.SetCounterValue(0);

            candidatePoolBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(SkinnedCandidate)), ComputeBufferType.Append);
            candidatePoolBuffer.SetCounterValue(0);

            edgePoolBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Edge)), ComputeBufferType.Append);
            edgePoolBuffer.SetCounterValue(0);

            var seeds = new List<Vector3>();
            for(int i = 0, n = Random.Range(2, 3); i < n; i++)
            {
                var attr = attractions[Random.Range(0, attractions.Length)];
                seeds.Add(attr.position);
            }

            // Setup(new Vector3[] { Vector3.zero });
            Setup(seeds.ToArray());

            Step();
            CopyNodesCount();
        }

        protected void Animate(float t, float dt)
        {
            var bones = skinnedRenderer.bones.Select(bone => bone.localToWorldMatrix).ToArray();
            using (ComputeBuffer boneMatrixBuffer = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4))))
            {
                boneMatrixBuffer.SetData(bones);

                var kernel = compute.FindKernel("Animate");
                compute.SetBuffer(kernel, "_BindPoses", bindPoseBuffer);
                compute.SetBuffer(kernel, "_BoneMatrices", boneMatrixBuffer);
                compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
                compute.SetFloat("_T", t);
                compute.SetFloat("_DT", dt);
                GPUHelper.Dispatch1D(compute, kernel, count);
            }
        }

        protected bool Parse(string line, out Vector3 result)
        {
            result = default(Vector3);
            var values = line.Split(' ');
            if(values.Length == 3)
            {
                var sx = values[0];
                var sy = values[1];
                var sz = values[2];
                float x, y, z;
                if(
                    float.TryParse(sx, out x) && 
                    float.TryParse(sy, out y) && 
                    float.TryParse(sz, out z)
                )
                {
                    result.Set(x, y, z);
                    return true;
                }
            }
            return false;
        }

        protected SkinnedAttraction[] GeneratePoints(TextAsset asset, float unit = 0.01f)
        {
            var lines = asset.text.Split('\n');

            var attractions = new List<SkinnedAttraction>();

            for(int i = 0, n = lines.Length; i < n; i++)
            {
                var line = lines[i];
                Vector3 p;
                if(Parse(line, out p))
                {
                    SkinnedAttraction attr = new SkinnedAttraction();
                    attr.position = p * unit;
                    attr.active = 1;
                    attr.found = 0;
                    attr.nearest = 0;
                    attractions.Add(attr);
                }
            }

            return attractions.ToArray();
        }

    }

}


