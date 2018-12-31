#ifndef __GEOMETRY__
#define __GEOMETRY__

struct AABB
{
  float3 min;
  float3 max;
};

struct Ray
{
  float3 origin;
  float3 dir;
  float3 invdir;
  int signs[3];

  bool intersects(AABB aabb)
  { 
    float tmin, tmax, tymin, tymax, tzmin, tzmax;

    float3 bounds[2];
    bounds[0] = aabb.min;
    bounds[1] = aabb.max;
   
    tmin = (bounds[signs[0]].x - origin.x) * invdir.x;
    tmax = (bounds[1 - signs[0]].x - origin.x) * invdir.x;
    tymin = (bounds[signs[1]].y - origin.y) * invdir.y;
    tymax = (bounds[1 - signs[1]].y - origin.y) * invdir.y;
   
    if ((tmin > tymax) || (tymin > tmax)) 
      return false;

    if (tymin > tmin) 
      tmin = tymin;

    if (tymax < tmax) 
      tmax = tymax;
   
    tzmin = (bounds[signs[2]].z - origin.z) * invdir.z;
    tzmax = (bounds[1 - signs[2]].z - origin.z) * invdir.z;
   
    if ((tmin > tzmax) || (tzmin > tmax)) 
      return false;

    if (tzmin > tmin) 
      tmin = tzmin;

    if (tzmax < tmax) 
      tmax = tzmax;

    return true; 
  } 

};

Ray CreateRay(float3 origin, float3 dir)
{
  Ray r;
  r.origin = origin;
  r.dir = dir;
  r.invdir = 1.0 / dir;
  r.signs[0] = (r.invdir.x < 0);
  r.signs[1] = (r.invdir.y < 0);
  r.signs[2] = (r.invdir.z < 0);
  return r;
}

float distance_line_to_point(float3 a, float3 b, float3 p)
{
  float3 d = normalize(b - a);
  float3 v = p - a;
  float t = dot(v, d);
  float3 pd = a + t * d; // p on ab
  return distance(pd, p);
}

#endif