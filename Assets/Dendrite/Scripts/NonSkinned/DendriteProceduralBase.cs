using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;


namespace Dendrite
{

    public abstract class DendriteProceduralBase : DendriteBase
    {

        public override DendriteType Type { get { return DendriteType.NonSkinned; } }
        public override Bounds Bounds { get { return new Bounds(Vector3.zero, Vector3.one); } }

        [SerializeField, Range(0f, 1f)] protected float randomize = 0.5f;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void Start() {
            base.Start();
            Reset();
        }
        
        protected override void Update () {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        protected abstract Attraction[] GenerateAttractions();

        protected override void Release()
        {
            base.Release();

            if (attractionBuffer == null) return;
            attractionBuffer.Dispose();
        }

        public override void Reset()
        {
            base.Reset();

            var attractions = GenerateAttractions();
            count = attractions.Length;

            attractionBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Attraction)), ComputeBufferType.Default);
            attractionBuffer.SetData(attractions);

            nodeBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Node)), ComputeBufferType.Default);
            nodePoolBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
            nodePoolBuffer.SetCounterValue(0);

            candidateBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Candidate)), ComputeBufferType.Append);
            candidateBuffer.SetCounterValue(0);

            edgeBuffer = new ComputeBuffer(count * 2, Marshal.SizeOf(typeof(Edge)), ComputeBufferType.Append);
            edgeBuffer.SetCounterValue(0);

            var seeds = Enumerable.Range(0, Random.Range(1, 5)).Select((_) => { return attractions[Random.Range(0, count)].position; }).ToArray();
            Setup(seeds);

            CopyNodesCount();
            CopyEdgesCount();
            Step(0f);
        }

    }

}


