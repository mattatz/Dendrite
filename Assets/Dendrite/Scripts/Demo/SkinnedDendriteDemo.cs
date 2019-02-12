using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite.Demo
{

    public class SkinnedDendriteDemo : MonoBehaviour
    {

        [SerializeField] protected CylindricalCamera cam;
        [SerializeField] protected DendriteBase dendrite;
        [SerializeField] protected DendriteEdgeRenderingOffset rendering;
        [SerializeField] protected Transform bone;
        [SerializeField] protected float duration = 5f;

        protected void Start()
        {
            StartCoroutine(IRepeater(duration));
        }

        protected void Update()
        {
            var center = dendrite.transform.TransformPoint(bone.position);
            cam.lookAt = center;
        }

        protected void Reset()
        {
            dendrite.Reset();
            rendering.Reset();
        }

        protected IEnumerator IRepeater(float duration)
        {
            yield return 0;
            while(true)
            {
                yield return new WaitForSeconds(duration);
                Reset();
            }
        }

    }

}


