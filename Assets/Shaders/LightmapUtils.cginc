#ifndef __LIGHTMAP_UTILS_INC__
#define __LIGHTMAP_UTILS_INC__

#define SampleAndDecodeLightmap(TEX, UV) \
	DecodeLightmap(UNITY_SAMPLE_TEX2D(TEX, UV))

#endif // __LIGHTMAP_UTILS_INC__
