using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Dendrite
{

    public class DendriteTree : DendriteProceduralBase
    {

        public override Bounds Bounds {
            get {
                var halfLength = (rootLength + branchLength) * 0.5f;
                var radius = Mathf.Max(branchRadiusBottom, branchRadiusTop);
                return new Bounds(
                    new Vector3(0f, halfLength, 0f), 
                    new Vector3(radius, halfLength, radius)
                );
            }
        }

        [SerializeField, Range(32, 100000)] protected int samples = 10000;
        [SerializeField] protected float rootLength = 4f;
        [SerializeField] protected float branchLength = 6f;
        [SerializeField] protected float branchRadiusBottom = 3f, branchRadiusTop = 9f;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            var root = Vector3.up * rootLength;
            var height = Vector3.up * branchLength;

            const int count = 32;
            for(int i = 0; i < count; i++)
            {
                var t = (1f * i) / count * Mathf.PI * 2f;
                var s = Mathf.Sin(t);
                var c = Mathf.Cos(t);
                var p0 = root + new Vector3(c, 0f, s) * branchRadiusBottom;
                var p1 = root + height + new Vector3(c, 0f, s) * branchRadiusTop;
                Gizmos.DrawLine(p0, p1);
            }
        }

        #endregion

        public override void Reset()
        {
            Release();

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

            // var seeds = Enumerable.Range(0, Random.Range(1, 5)).Select((_) => { return attractions[Random.Range(0, count)].position; }).ToArray();
            Setup(new Vector3[] { Vector3.zero });

            CopyNodesCount();
            CopyEdgesCount();
            Step(0f);
        }


        // https://stackoverflow.com/questions/41749411/uniform-sampling-by-volume-within-a-cone
        protected override Attraction[] GenerateAttractions()
        {
            var attractions = new List<Attraction>();

            for(int i = 0; i < samples; i++)
            {
                var h = branchLength * Mathf.Pow(Random.value, 1f / 3f);
                var r = Mathf.Lerp(branchRadiusBottom, branchRadiusTop, h / branchLength) * Mathf.Sqrt(Random.value);
                var t = 2f * Mathf.PI * Random.value;

                var p = new Vector3(
                    Mathf.Cos(t) * r,
                    rootLength + h,
                    Mathf.Sin(t) * r
                );

                Attraction attr;
                {
                    attr.position = p;
                    attr.active = 1;
                    attr.found = 0;
                    attr.nearest = 0;
                }
                attractions.Add(attr);
            }

            return attractions.ToArray();
        }

    }

}


