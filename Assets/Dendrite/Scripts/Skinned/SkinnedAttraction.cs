using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Dendrite
{
    
    [StructLayout (LayoutKind.Sequential)]
    public struct SkinnedAttraction {
        public Vector3 position;
        public int bone;
        public int nearest;
        public uint found;
        public uint active;
    }

}


