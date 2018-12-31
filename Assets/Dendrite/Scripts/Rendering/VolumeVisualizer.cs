using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    [RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
    public class VolumeVisualizer : MonoBehaviour
    {

        public Texture Volume {
            set {
                if(block == null)
                {
                    block = new MaterialPropertyBlock();
                    var renderer = GetComponent<Renderer>();
                    renderer.GetPropertyBlock(block);
                    block.SetTexture("_Volume", value);
                    renderer.SetPropertyBlock(block);
                }
            }
        }

        [Header ("Visualize")]
        [SerializeField, Range(16, 128)] protected int depth = 32;
        protected MaterialPropertyBlock block;

        protected void OnEnable()
        {
            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = Build(depth);

        }

        protected Mesh Build(int depth = 64)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();

            for (int i = 0; i < depth; i++)
            {
                var t01 = 1f * i / (depth - 1);
                float z = t01 - 0.5f;

                vertices.Add(new Vector3(-0.5f, -0.5f, z));
                vertices.Add(new Vector3( 0.5f, -0.5f, z));
                vertices.Add(new Vector3( 0.5f,  0.5f, z));
                vertices.Add(new Vector3(-0.5f,  0.5f, z));
                normals.Add(new Vector3(0f, 0f, t01));
                normals.Add(new Vector3(1f, 0f, t01));
                normals.Add(new Vector3(1f, 1f, t01));
                normals.Add(new Vector3(0f, 1f, t01));

                int idx = i * 4;
                indices.Add(idx); indices.Add(idx + 2); indices.Add(idx + 1);
                indices.Add(idx + 2); indices.Add(idx); indices.Add(idx + 3);
            }

            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }



    }

}


