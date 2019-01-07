using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeSampler
{

    public class Volume : ScriptableObject
    {

        public List<Vector3> Points { get { return points; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Depth { get { return depth; } }
        public float UnitLength { get { return unitLength; } }

        [SerializeField] protected int width, height, depth;
        [SerializeField] protected float unitLength;
        [SerializeField] protected List<Vector3> points;

        public void Initialize(int w, int h, int d, float u, List<Vector3> pts)
        {
            width = w;
            height = h;
            depth = d;
            unitLength = u;
            points = pts;
        }

    }

}


