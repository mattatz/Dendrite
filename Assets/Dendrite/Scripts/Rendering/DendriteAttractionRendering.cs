using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    public class DendriteAttractionRendering : DendritePointRenderingBase
    {

        protected override void Render(float extents = 100)
        {
            material.SetBuffer("_Attractions", dendrite.AttractionBuffer);
            material.SetMatrix("_World2Local", transform.worldToLocalMatrix);
            material.SetMatrix("_Local2World", transform.localToWorldMatrix);
            material.SetPass(0);
            Graphics.DrawMeshInstancedIndirect(quad, 0, material, new Bounds(Vector3.zero, Vector3.one * extents), drawBuffer);

        }

    }

}


