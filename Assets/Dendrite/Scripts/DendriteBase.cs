using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Dendrite
{

    public enum DendriteType
    {
        NonSkinned,
        Skinned,
    };

    public abstract class DendriteBase : MonoBehaviour
    {

        public float InfluenceDistance { get { return influenceDistance; } set { influenceDistance = value; } }
        public float GrowthDistance { get { return growthDistance; } set { growthDistance = value; } }
        public float KillDistance { get { return killDistance; } set { killDistance = value; } }
        public float GrowthSpeed { get { return growthSpeed; } set { growthSpeed = value; } }

        public abstract DendriteType Type { get; }
        public abstract Bounds Bounds { get; }

        public ComputeBuffer AttractionBuffer { get { return attractionBuffer; } }
        public ComputeBuffer NodeBuffer { get { return nodeBuffer; } }
        public ComputeBuffer EdgeBuffer { get { return edgePoolBuffer; } }
        public int Count { get { return count; } }
        public int NodesCount { get { return nodesCount; } }
        public int EdgesCount { get { return edgesCount; } }

        [SerializeField] protected ComputeShader compute;

        protected ComputeBuffer attractionBuffer, nodeBuffer;
        protected ComputeBuffer edgePoolBuffer, nodePoolBuffer, candidatePoolBuffer;

        protected ComputeBuffer poolArgsBuffer;
        protected int[] poolArgs = new int[] { 0, 1, 0, 0 };

        [SerializeField] protected float unitDistance = 0.0025f;

        [SerializeField] protected int count;
        [SerializeField] protected int nodesCount, edgesCount;

        [SerializeField, Range(1, 5)] protected int iterations = 1;
        [SerializeField, Range(0f, 1f)] protected float massMin = 0.25f, massMax = 1f;
        [SerializeField, Range(0.25f, 3f)] protected float influenceDistance = 0.25f;
        [SerializeField, Range(0.25f, 1f)] protected float growthDistance = 0.2f, killDistance = 0.2f;
        [SerializeField] protected float growthSpeed = 22f;

        #region MonoBehaviour

        protected virtual void OnEnable()
        {
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start() {
            poolArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        }
        
        protected virtual void Update () {
            for (int i = 0; i < iterations; i++)
                Step(Time.deltaTime);
        }

        protected virtual void OnDestroy()
        {
            Release();
            attractionBuffer.Dispose();
            poolArgsBuffer.Dispose();
        }

        #endregion

        protected void Setup(Vector3[] points)
        {
            var kernel = compute.FindKernel("Setup");
            compute.SetBuffer(kernel, "_NodesPoolAppend", nodePoolBuffer);
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
            GPUHelper.Dispatch1D(compute, kernel, count);

            using(ComputeBuffer seeds = new ComputeBuffer(points.Length, Marshal.SizeOf(typeof(Vector3))))
            {
                seeds.SetData(points);
                kernel = compute.FindKernel("Seed");
                compute.SetFloat("_MassMin", massMin);
                compute.SetFloat("_MassMax", massMax);
                compute.SetBuffer(kernel, "_Seeds", seeds);
                compute.SetBuffer(kernel, "_Attractions", attractionBuffer);
                compute.SetBuffer(kernel, "_NodesPoolConsume", nodePoolBuffer);
                compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
                GPUHelper.Dispatch1D(compute, kernel, seeds.count);
            }

            nodesCount = nodePoolBuffer.count;
            edgesCount = 0;
        }

        protected virtual void Release()
        {
            if (nodeBuffer == null) return;

            nodeBuffer.Dispose();
            nodePoolBuffer.Dispose();
            candidatePoolBuffer.Dispose();
            edgePoolBuffer.Dispose();
        }

        public virtual void Reset()
        {
            Release();
        }

        protected void Step(float dt)
        {
            Grow(dt);
            Constrain();
            if (nodesCount > 0)
            {
                Search();
                Attract();
                Connect();
                Remove();

                CopyNodesCount();
                CopyEdgesCount();
            }
        }

        protected void Search()
        {
            var kernel = compute.FindKernel("Search");
            compute.SetBuffer(kernel, "_Attractions", attractionBuffer);
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
            compute.SetFloat("_InfluenceDistance", unitDistance * influenceDistance);
            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Grow(float dt)
        {
            var kernel = compute.FindKernel("Grow");
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);

            var delta = dt * growthSpeed;
            compute.SetFloat("_DT", delta);

            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Attract()
        {
            var kernel = compute.FindKernel("Attract");
            compute.SetBuffer(kernel, "_Attractions", attractionBuffer);
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);

            candidatePoolBuffer.SetCounterValue(0);
            compute.SetBuffer(kernel, "_CandidatesPoolAppend", candidatePoolBuffer);

            compute.SetFloat("_GrowthDistance", unitDistance * growthDistance);

            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Connect()
        {
            var kernel = compute.FindKernel("Connect");
            compute.SetFloat("_MassMin", massMin);
            compute.SetFloat("_MassMax", massMax);
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
            compute.SetBuffer(kernel, "_EdgesPoolAppend", edgePoolBuffer);
            compute.SetBuffer(kernel, "_NodesPoolConsume", nodePoolBuffer);
            compute.SetBuffer(kernel, "_CandidatesPoolConsume", candidatePoolBuffer);

            var connectCount = Mathf.Min(nodesCount, CopyPoolCount(candidatePoolBuffer));
            if (connectCount > 0)
            {
                compute.SetInt("_ConnectCount", connectCount);
                GPUHelper.Dispatch1D(compute, kernel, connectCount);
            }
        }

        protected void Remove()
        {
            var kernel = compute.FindKernel("Remove");
            compute.SetBuffer(kernel, "_Attractions", attractionBuffer);
            compute.SetBuffer(kernel, "_Nodes", nodeBuffer);
            compute.SetFloat("_KillDistance", unitDistance * killDistance);
            GPUHelper.Dispatch1D(compute, kernel, count);
        }

        protected void Constrain()
        {
            killDistance = Mathf.Min(growthDistance, killDistance);
        }

        protected int CopyNodesCount()
        {
            return nodesCount = CopyPoolCount(nodePoolBuffer);
        }

        protected int CopyEdgesCount()
        {
            return edgesCount = CopyPoolCount(edgePoolBuffer);
        }

        protected int CopyPoolCount(ComputeBuffer buffer)
        {
            poolArgsBuffer.SetData(poolArgs);
            ComputeBuffer.CopyCount(buffer, poolArgsBuffer, 0);
            poolArgsBuffer.GetData(poolArgs);
            return poolArgs[0];
        }

    }

}


