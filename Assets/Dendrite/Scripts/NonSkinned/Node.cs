using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Dendrite
{

    [StructLayout (LayoutKind.Sequential)]
    public struct Node {
        public Vector3 position;
        public float t;
        public float offset;
        public float mass;
        public int from;
        public uint active;
    }

}


