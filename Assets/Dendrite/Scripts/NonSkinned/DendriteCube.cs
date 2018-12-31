using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using Random = UnityEngine.Random;

namespace Dendrite
{

    public class DendriteCube : DendriteProceduralBase {

        [SerializeField] protected int width = 16, height = 16, depth = 16;
        [SerializeField] protected bool normalize;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            base.OnEnable();
            unitDistance = Mathf.Min(Mathf.Min(1f / width, 1f / height), 1f / depth);
        }
       
        #endregion

        protected override Attraction[] GenerateAttractions()
        {
            float invW, invH, invD;
            if(normalize)
            {
                invW = 1f / (width - 1);
                invH = 1f / (height - 1);
                invD = 1f / (depth - 1);
            } else
            {
                var m = Mathf.Max(width, height, depth);
                invW = invH = invD = 1f / (m - 1);
            }

            var offset = - new Vector3(0.5f, 0.5f, 0.5f);
            var scale = new Vector3(invW, invH, invD);

            var attractions = new Attraction[width * height * depth];
            for(int z = 0; z < depth; z++)
            {
                var zoff = z * (width * height);
                for(int y = 0; y < height; y++)
                {
                    var yoff = y * width;
                    for(int x = 0; x < width; x++)
                    {
                        var idx = x + yoff + zoff;
                        var attr = attractions[idx];

                        attr.position = 
                            Vector3.Scale(new Vector3(x, y, z), scale)
                            + Vector3.Scale(randomize * new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)), scale)
                            + offset;

                        attr.active = 1;
                        attr.found = 0;
                        attr.nearest = 0;
                        attractions[idx] = attr;
                    }
                }
            }
            return attractions;
        }

    }

}


