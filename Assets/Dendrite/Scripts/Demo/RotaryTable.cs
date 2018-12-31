using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dendrite.Demo
{

    public class RotaryTable : MonoBehaviour
    {

        [SerializeField] protected float delta = 10f;

        protected void Update()
        {
            transform.RotateAround(transform.position, transform.up, Time.deltaTime * delta);
        }

    }

}


