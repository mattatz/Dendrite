#ifndef __NODE_INCLUDED__
#define __NODE_INCLUDED__

struct SkinnedNode
{
  float3 position;
  float3 animated;
  int index0;
  float t;
  float offset;
  float mass;
  int from;
  bool active;
};

#endif
