using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Dendrite
{

    public class DendriteSphere : DendriteProceduralBase {

        [SerializeField] protected int side = 16;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            base.OnEnable();
            unitDistance = Mathf.Min(Mathf.Min(1f / side, 1f / side), 1f / side) * 2f;
        }
       
        #endregion

        protected override Attraction[] GenerateAttractions()
        {
            float invW = 1f / (side - 1), invH = 1f / (side - 1), invD = 1f / (side - 1);
            var offset = - new Vector3(0.5f, 0.5f, 0.5f);
            var scale = new Vector3(invW, invH, invD);

            var attractions = new List<Attraction>();
            for(int z = 0; z < side; z++)
            {
                for(int y = 0; y < side; y++)
                {
                    for(int x = 0; x < side; x++)
                    {
                        var p = Vector3.Scale(new Vector3(x, y, z), scale) + offset;
                        if (p.sqrMagnitude >= 0.25f) continue;

                        Attraction attr;
                        {
                            attr.position = p + Vector3.Scale(randomize * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), scale);
                            attr.active = 1;
                            attr.found = 0;
                            attr.nearest = 0;
                        }
                        attractions.Add(attr);
                    }
                }
            }

            return attractions.ToArray();
        }

    }

}


