#ifndef __ATTRACTION_INCLUDED__
#define __ATTRACTION_INCLUDED__

struct SkinnedAttraction
{
  float3 position;
  uint bone;
  uint nearest;
  bool found;
  bool active;
};

#endif
