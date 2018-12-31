using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;

namespace Dendrite
{

    [System.Serializable]
    public class VolumeEvent : UnityEvent<Texture> {}

    public class Volume : MonoBehaviour
    {
        public RenderTexture Tex { get { return buffer; } }
        public DendriteBase Dendrite { get { return dendrite; } set { dendrite = value; } }

        [SerializeField] protected ComputeShader compute;
        [SerializeField, Range(16, 512)] protected int size = 64;
        [SerializeField, Range(1f, 20f)] protected float thickness = 2f;

        [SerializeField] protected DendriteBase dendrite;
        protected Bounds bounds { get { return dendrite.Bounds; } }

        [SerializeField] protected RenderTexture buffer;
        [SerializeField] protected VolumeEvent onUpdate;

        #region MonoBehaviour

        protected void OnEnable()
        {
            buffer = Create(size);
        }

        protected void Start()
        {
            Clear();
            onUpdate.Invoke(buffer);
        }

        protected void Update()
        {
            if (dendrite.Type == DendriteType.Skinned)
            {
                Clear();

                // Bake twice for Skinned type
                // to prevent flickering volume by data race.
                // (It is not completely prevented.)
                Bake(); 
            }

            Bake();
        }

        protected void OnDestroy()
        {
            if (buffer != null) buffer.Release();
        }

        #endregion

        public void Clear()
        {
            var kernel = compute.FindKernel("Clear");
            compute.SetTexture(kernel, "_Volume", buffer);
            GPUHelper.Dispatch3D(compute, kernel, size, size, size);
        }

        protected void Bake()
        {
            var kernel = compute.FindKernel((dendrite.Type == DendriteType.Skinned) ? "BakeSkinned" : "Bake");
            compute.SetTexture(kernel, "_Volume", buffer);

            var max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            compute.SetVector("_Min", bounds.min + new Vector3(bounds.size.x - max, bounds.size.y - max, bounds.size.z - max) * 0.5f);
            compute.SetVector("_Size", Vector3.one * max);

            compute.SetFloat("_Thickness", thickness);
            compute.SetBuffer(kernel, "_Edges", dendrite.EdgeBuffer);
            compute.SetBuffer(kernel, "_Nodes", dendrite.NodeBuffer);
            compute.SetInt("_EdgesCount", dendrite.EdgesCount);
            GPUHelper.Dispatch1D(compute, kernel, dendrite.EdgesCount);
        }

        protected RenderTexture Create(int size)
        {
            var t = new RenderTexture(size, size, 0, RenderTextureFormat.RFloat);
            t.dimension = TextureDimension.Tex3D;
            t.volumeDepth = size;
            t.wrapMode = TextureWrapMode.Clamp;
            t.enableRandomWrite = true;
            t.Create();
            return t;
        }

    }

}


