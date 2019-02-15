using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

using VolumeSampler;

namespace Dendrite
{

    public class SkinnedDendrite : DendriteBase {

        public override DendriteType Type { get { return DendriteType.Skinned; } }
        public override Bounds Bounds { get { return skinnedRenderer.sharedMesh.bounds; } }

        [SerializeField, Range(1f, 10f)] protected float unitScale = 1f;

        [SerializeField] protected VolumeSampler.Volume volume;
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

            attractions = GeneratePoints(volume);
            attractionBuffer = new ComputeBuffer(attractions.Length, Marshal.SizeOf(typeof(SkinnedAttraction)), ComputeBufferType.Default);
            attractionBuffer.SetData(attractions);

            unitDistance = volume.UnitLength * unitScale;

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

            candidateBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(SkinnedCandidate)), ComputeBufferType.Append);
            candidateBuffer.SetCounterValue(0);

            edgeBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Edge)), ComputeBufferType.Append);
            edgeBuffer.SetCounterValue(0);

            var seeds = new List<Vector3>();
            for(int i = 0, n = Random.Range(4, 5); i < n; i++)
            {
                var attr = attractions[Random.Range(0, attractions.Length)];
                seeds.Add(attr.position);
            }
            Setup(seeds.ToArray());

            CopyNodesCount();
            CopyEdgesCount();
            Step(0f);
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

        protected SkinnedAttraction[] GeneratePoints(VolumeSampler.Volume volume)
        {
            var count = volume.Points.Count;
            var attractions = new SkinnedAttraction[volume.Points.Count];
            for(int i = 0; i < count; i++)
            {
                var p = volume.Points[i];
                var attr = attractions[i];
                attr.position = p;
                attr.active = 1;
                attractions[i] = attr;
            }
            return attractions;
        }

    }

}


