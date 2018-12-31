using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite.Demo
{

    public class Rotator : MonoBehaviour
    {

        [SerializeField] protected float speed = 1f, delta = 10f;

        protected void Update()
        {
            var t = Time.timeSinceLevelLoad * speed;
            var axis = new Vector3(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(-100f, t) - 0.5f,
                Mathf.PerlinNoise(t + 100f, t) - 0.5f
            );
            transform.RotateAround(transform.position, axis.normalized, Time.deltaTime * delta);
        }

    }

}


