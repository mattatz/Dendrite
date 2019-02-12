using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    public class DendriteEdgeRenderingOffset : DendriteEdgeRendering
    {

        public float Offset { get { return offset; } set { offset = value; } }

        [SerializeField] protected float offset = 0f;
        [SerializeField] protected float speed = 1f;

        protected override void Update()
        {
            offset += Time.deltaTime * speed;
            block.SetFloat("_Offset", offset);
            base.Update();
        }

        public void Reset()
        {
            offset = 0f;
        }

    }

}


