using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeSampler
{

    // References:
    // https://github.com/mattatz/unity-voxel
    // https://github.com/njanakiev/poisson-disk-sampling

    public static class VolumeSampler
    {

        public static Volume Sample(Mesh mesh, int resolution, float r = 1f, int tries = 8)
        {
            int width, height, depth;
            float unit;
            var grids = Voxelize(mesh, resolution, out width, out height, out depth, out unit);

            var radius = r * unit;
            var radius2 = radius * 2f;

            var actives = new List<Vector3>();
            var min = mesh.bounds.min;
            var max = mesh.bounds.max;
            var size = mesh.bounds.size;

            Vector3 p = default(Vector3);
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (grids[x, y, z].fill)
                        {
                            p = grids[x, y, z].position;
                            break;
                        }
                    }
                }
            }

            var hunit = unit * 0.5f;
            p.x = Mathf.Clamp(p.x, min.x + hunit, max.x - hunit);
            p.y = Mathf.Clamp(p.y, min.y + hunit, max.y - hunit);
            p.z = Mathf.Clamp(p.z, min.z + hunit, max.z - hunit);
            actives.Add(p);

            int col, row, layer;
            GetIndex(p, min, size, width, height, depth, out col, out row, out layer);
            grids[col, row, layer].Sample(p);

            var R = Mathf.Min(size.x, size.y, size.z);

            while (actives.Count > 0)
            {
                var found = false;

                int idx = Random.Range(0, actives.Count);
                var pos = actives[idx];

                for (int i = 0; i < tries; i++)
                {
                    var sample = pos + Random.insideUnitSphere.normalized * Random.Range(radius, radius2);
                    GetIndex(sample, min, size, width, height, depth, out col, out row, out layer);

                    var insideGrid = 0 <= col && col < width && 0 <= row && row < height && 0 <= layer && layer < depth;
                    if (insideGrid && grids[col, row, layer].fill &&!grids[col, row, layer].found)
                    {
                        var ok = true;

                        // check neighbors
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    int x = col + dx;
                                    int y = row + dy;
                                    int z = layer + dz;
                                    if (0 <= x && x < width && 0 <= y && y < height && 0 <= z && z < depth)
                                    {
                                        var neighbor = grids[x, y, z];
                                        if (neighbor.found)
                                        {
                                            if (Vector3.Distance(neighbor.sample, sample) < radius)
                                            {
                                                ok = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (ok)
                        {
                            found = true;
                            grids[col, row, layer].Sample(sample);
                            actives.Add(sample);
                            break;
                        }
                    }
                }

                if (!found)
                {
                    actives.RemoveAt(idx);
                }
            }

            var points = new List<Vector3>();
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var grid = grids[x, y, z];
                        if (grid.found)
                        {
                            points.Add(grid.sample);
                        }
                    }
                }
            }

            var volume = ScriptableObject.CreateInstance<Volume>();
            volume.Initialize(width, height, depth, unit, points);
            return volume;
        }

        static void GetIndex(Vector3 p, Vector3 min, Vector3 size, int w, int h, int d, out int x, out int y, out int z)
        {
            x = Mathf.FloorToInt((p.x - min.x) / size.x * w);
            y = Mathf.FloorToInt((p.y - min.y) / size.y * h);
            z = Mathf.FloorToInt((p.z - min.z) / size.z * d);
        }

        #region Voxelizer

        public static Grid[,,] Voxelize(Mesh mesh, int resolution, out int width, out int height, out int depth, out float unit)
        {
            mesh.RecalculateBounds();

            var bounds = mesh.bounds;
            float maxLength = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            unit = maxLength / resolution;
            var hunit = unit * 0.5f;

            var start = bounds.min - new Vector3(hunit, hunit, hunit);
            var end = bounds.max + new Vector3(hunit, hunit, hunit);
            var size = end - start;

            width = Mathf.CeilToInt(size.x / unit);
            height = Mathf.CeilToInt(size.y / unit);
            depth = Mathf.CeilToInt(size.z / unit);

            var volume = new Grid[width, height, depth];
            var boxes = new Bounds[width, height, depth];
            var voxelSize = Vector3.one * unit;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        var p = new Vector3(x, y, z) * unit + start;
                        var aabb = new Bounds(p, voxelSize);
                        boxes[x, y, z] = aabb;
                    }
                }
            }

            // build triangles
            var vertices = mesh.vertices;
            var indices = mesh.triangles;
            var direction = Vector3.forward;

            for (int i = 0, n = indices.Length; i < n; i += 3)
            {
                var tri = new Triangle(
                    vertices[indices[i]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]],
                    direction
                );

                var min = tri.bounds.min - start;
                var max = tri.bounds.max - start;
                int iminX = Mathf.RoundToInt(min.x / unit), iminY = Mathf.RoundToInt(min.y / unit), iminZ = Mathf.RoundToInt(min.z / unit);
                int imaxX = Mathf.RoundToInt(max.x / unit), imaxY = Mathf.RoundToInt(max.y / unit), imaxZ = Mathf.RoundToInt(max.z / unit);
                // int iminX = Mathf.FloorToInt(min.x / unit), iminY = Mathf.FloorToInt(min.y / unit), iminZ = Mathf.FloorToInt(min.z / unit);
                // int imaxX = Mathf.CeilToInt(max.x / unit), imaxY = Mathf.CeilToInt(max.y / unit), imaxZ = Mathf.CeilToInt(max.z / unit);

                iminX = Mathf.Clamp(iminX, 0, width - 1);
                iminY = Mathf.Clamp(iminY, 0, height - 1);
                iminZ = Mathf.Clamp(iminZ, 0, depth - 1);
                imaxX = Mathf.Clamp(imaxX, 0, width - 1);
                imaxY = Mathf.Clamp(imaxY, 0, height - 1);
                imaxZ = Mathf.Clamp(imaxZ, 0, depth - 1);

                var front = tri.frontFacing;

                for (int x = iminX; x <= imaxX; x++)
                {
                    for (int y = iminY; y <= imaxY; y++)
                    {
                        for (int z = iminZ; z <= imaxZ; z++)
                        {
                            if (Intersects(tri, boxes[x, y, z]))
                            {
                                var voxel = volume[x, y, z];
                                voxel.position = boxes[x, y, z].center;
                                if (!voxel.fill)
                                {
                                    voxel.front = front;
                                }
                                else
                                {
                                    voxel.front = voxel.front || front;
                                }
                                voxel.fill = true;
                                volume[x, y, z] = voxel;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (volume[x, y, z].IsEmpty()) continue;

                        int ifront = z;

                        for (; ifront < depth; ifront++)
                        {
                            if (!volume[x, y, ifront].IsFrontFace())
                            {
                                break;
                            }
                        }

                        if (ifront >= depth) break;

                        var iback = ifront;

                        // step forward to cavity
                        for (; iback < depth && volume[x, y, iback].IsEmpty(); iback++) { }

                        if (iback >= depth) break;

                        // check if iback is back voxel
                        if (volume[x, y, iback].IsBackFace())
                        {
                            // step forward to back face
                            for (; iback < depth && volume[x, y, iback].IsBackFace(); iback++) { }
                        }

                        // fill from ifront to iback
                        for (int z2 = ifront; z2 < iback; z2++)
                        {
                            var p = boxes[x, y, z2].center;
                            var voxel = volume[x, y, z2];
                            voxel.position = p;
                            voxel.fill = true;
                            volume[x, y, z2] = voxel;
                        }

                        z = iback;
                    }
                }
            }

            return volume;
        }

        static bool Intersects(Triangle tri, Bounds aabb)
        {
            float p0, p1, p2, r;

            Vector3 center = aabb.center, extents = aabb.max - center;

            Vector3 v0 = tri.a - center,
                v1 = tri.b - center,
                v2 = tri.c - center;

            Vector3 f0 = v1 - v0,
                f1 = v2 - v1,
                f2 = v0 - v2;

            Vector3 a00 = new Vector3(0, -f0.z, f0.y),
                a01 = new Vector3(0, -f1.z, f1.y),
                a02 = new Vector3(0, -f2.z, f2.y),
                a10 = new Vector3(f0.z, 0, -f0.x),
                a11 = new Vector3(f1.z, 0, -f1.x),
                a12 = new Vector3(f2.z, 0, -f2.x),
                a20 = new Vector3(-f0.y, f0.x, 0),
                a21 = new Vector3(-f1.y, f1.x, 0),
                a22 = new Vector3(-f2.y, f2.x, 0);

            // Test axis a00
            p0 = Vector3.Dot(v0, a00);
            p1 = Vector3.Dot(v1, a00);
            p2 = Vector3.Dot(v2, a00);
            r = extents.y * Mathf.Abs(f0.z) + extents.z * Mathf.Abs(f0.y);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a01
            p0 = Vector3.Dot(v0, a01);
            p1 = Vector3.Dot(v1, a01);
            p2 = Vector3.Dot(v2, a01);
            r = extents.y * Mathf.Abs(f1.z) + extents.z * Mathf.Abs(f1.y);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a02
            p0 = Vector3.Dot(v0, a02);
            p1 = Vector3.Dot(v1, a02);
            p2 = Vector3.Dot(v2, a02);
            r = extents.y * Mathf.Abs(f2.z) + extents.z * Mathf.Abs(f2.y);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a10
            p0 = Vector3.Dot(v0, a10);
            p1 = Vector3.Dot(v1, a10);
            p2 = Vector3.Dot(v2, a10);
            r = extents.x * Mathf.Abs(f0.z) + extents.z * Mathf.Abs(f0.x);
            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a11
            p0 = Vector3.Dot(v0, a11);
            p1 = Vector3.Dot(v1, a11);
            p2 = Vector3.Dot(v2, a11);
            r = extents.x * Mathf.Abs(f1.z) + extents.z * Mathf.Abs(f1.x);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a12
            p0 = Vector3.Dot(v0, a12);
            p1 = Vector3.Dot(v1, a12);
            p2 = Vector3.Dot(v2, a12);
            r = extents.x * Mathf.Abs(f2.z) + extents.z * Mathf.Abs(f2.x);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a20
            p0 = Vector3.Dot(v0, a20);
            p1 = Vector3.Dot(v1, a20);
            p2 = Vector3.Dot(v2, a20);
            r = extents.x * Mathf.Abs(f0.y) + extents.y * Mathf.Abs(f0.x);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a21
            p0 = Vector3.Dot(v0, a21);
            p1 = Vector3.Dot(v1, a21);
            p2 = Vector3.Dot(v2, a21);
            r = extents.x * Mathf.Abs(f1.y) + extents.y * Mathf.Abs(f1.x);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            // Test axis a22
            p0 = Vector3.Dot(v0, a22);
            p1 = Vector3.Dot(v1, a22);
            p2 = Vector3.Dot(v2, a22);
            r = extents.x * Mathf.Abs(f2.y) + extents.y * Mathf.Abs(f2.x);

            if (Mathf.Max(-Mathf.Max(p0, p1, p2), Mathf.Min(p0, p1, p2)) > r)
            {
                return false;
            }

            if (Mathf.Max(v0.x, v1.x, v2.x) < -extents.x || Mathf.Min(v0.x, v1.x, v2.x) > extents.x)
            {
                return false;
            }

            if (Mathf.Max(v0.y, v1.y, v2.y) < -extents.y || Mathf.Min(v0.y, v1.y, v2.y) > extents.y)
            {
                return false;
            }

            if (Mathf.Max(v0.z, v1.z, v2.z) < -extents.z || Mathf.Min(v0.z, v1.z, v2.z) > extents.z)
            {
                return false;
            }

            var normal = Vector3.Cross(f1, f0).normalized;
            var pl = new Plane(normal, Vector3.Dot(normal, tri.a));
            return Intersects(pl, aabb);
        }

        static bool Intersects(Plane pl, Bounds aabb)
        {
            Vector3 center = aabb.center;
            var extents = aabb.max - center;

            var r = extents.x * Mathf.Abs(pl.normal.x) + extents.y * Mathf.Abs(pl.normal.y) + extents.z * Mathf.Abs(pl.normal.z);
            var s = Vector3.Dot(pl.normal, center) - pl.distance;

            return Mathf.Abs(s) <= r;
        }

        #endregion

        #region Classes

        public struct Grid
        {
            public Vector3 position;
            public bool fill, front;

            public Vector3 sample;
            public bool found;

            public bool IsFrontFace()
            {
                return fill && front;
            }

            public bool IsBackFace()
            {
                return fill && !front;
            }

            public bool IsEmpty()
            {
                return !fill;
            }

            public void Sample(Vector3 p)
            {
                found = true;
                sample = p;
            }
        }

        public class Triangle
        {
            public Vector3 a, b, c;
            public Bounds bounds;
            public bool frontFacing;

            public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 dir)
            {
                this.a = a;
                this.b = b;
                this.c = c;

                var cross = Vector3.Cross(b - a, c - a);
                this.frontFacing = (Vector3.Dot(cross, dir) <= 0f);

                var min = Vector3.Min(Vector3.Min(a, b), c);
                var max = Vector3.Max(Vector3.Max(a, b), c);
                bounds.SetMinMax(min, max);
            }

        }

        #endregion

    }

}


