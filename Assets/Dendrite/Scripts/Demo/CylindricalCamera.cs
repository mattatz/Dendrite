using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite.Demo
{

    public class CylindricalCamera : MonoBehaviour
    {

        public Vector3 lookAt;
        protected Vector3 prevLookAt;

        public float distance = 5f, height = 3f;
        public float theta;

        public float lookAtSpeed = 1f, rotate = 1f;

        protected void OnEnable()
        {
            prevLookAt = lookAt;
        }

        protected void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            theta += dt;
            var c = Mathf.Cos(theta) * distance;
            var s = Mathf.Sin(theta) * distance;

            prevLookAt = Vector3.Lerp(prevLookAt, lookAt, dt);
            transform.position = prevLookAt + new Vector3(c, height, s);
            transform.LookAt(prevLookAt);
        }

    }

}


