using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    public abstract class DendritePointRenderingBase : DendriteRenderingBase
    {

        [SerializeField] protected Material material;

        protected ComputeBuffer drawBuffer;
        protected uint[] 
            drawArgs = new uint[5] { 0, 0, 0, 0, 0 };

        protected Mesh quad;

        #region MonoBehaviour

        protected void OnEnable()
        {
            quad = BuildQuad();
        }

        protected void Update()
        {
            SetupDrawArgumentsBuffers(dendrite.Count);
            Render();
        }

        protected void OnDestroy()
        {
            if (drawBuffer != null) drawBuffer.Dispose();
        }

        #endregion

        protected void SetupDrawArgumentsBuffers(int count)
        {
            if (drawArgs[1] == (uint)count) return;

            drawArgs[0] = quad.GetIndexCount(0);
            drawArgs[1] = (uint)count;

            if (drawBuffer != null) drawBuffer.Dispose();
            drawBuffer = new ComputeBuffer(1, sizeof(uint) * drawArgs.Length, ComputeBufferType.IndirectArguments);
            drawBuffer.SetData(drawArgs);
        }

        protected abstract void Render(float extents = 100f);

        protected Mesh BuildQuad()
        {
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            mesh.vertices = new Vector3[4] {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3( 0.5f, -0.5f, 0f),
                new Vector3( 0.5f,  0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f)
            };
            mesh.uv = new Vector2[4] {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.SetIndices(new int[6] { 0, 2, 1, 2, 0, 3 }, MeshTopology.Triangles, 0);
            return mesh;
        }

    }

}


