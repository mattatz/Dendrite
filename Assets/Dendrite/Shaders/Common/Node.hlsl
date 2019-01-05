#ifndef __NODE_INCLUDED__
#define __NODE_INCLUDED__

struct Node
{
  float3 position;
  float t;
  float offset;
  float mass;
  int from;
  bool active;
};

#endif
