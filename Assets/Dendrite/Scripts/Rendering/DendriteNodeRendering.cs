using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    public class DendriteNodeRendering : DendritePointRenderingBase
    {

        protected override void Render(float extents = 100f)
        {
            material.SetBuffer("_Nodes", dendrite.NodeBuffer);
            material.SetMatrix("_World2Local", transform.worldToLocalMatrix);
            material.SetMatrix("_Local2World", transform.localToWorldMatrix);
            material.SetPass(0);
            Graphics.DrawMeshInstancedIndirect(quad, 0, material, new Bounds(Vector3.zero, Vector3.one * extents), drawBuffer);
        }

    }

}


