using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite
{

    public abstract class DendriteRenderingBase : MonoBehaviour
    {

        public DendriteBase Dendrite { get { return dendrite; } set { dendrite = value; } }

        [SerializeField] protected DendriteBase dendrite;

        public virtual void Clear()
        {
        }

    }

}


