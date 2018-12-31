#ifndef __ATTRACTION_INCLUDED__
#define __ATTRACTION_INCLUDED__

struct Attraction
{
  float3 position;
  uint nearest;
  bool found;
  bool active;
};

#endif
