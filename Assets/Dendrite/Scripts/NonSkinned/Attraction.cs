using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Dendrite
{
    
    [StructLayout (LayoutKind.Sequential)]
    public struct Attraction {
        public Vector3 position;
        public int nearest;
        public uint found;
        public uint active;
    }

}


