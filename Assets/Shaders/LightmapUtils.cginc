#ifndef __LIGHTMAP_UTILS_INC__
#define __LIGHTMAP_UTILS_INC__

#define SampleAndDecodeLightmap(TEX, UV) \
	DecodeLightmap(UNITY_SAMPLE_TEX2D(TEX, UV))

#define SampleAndDecodeLightmapLOD(TEX, UV, LODN) \
	DecodeLightmap(UNITY_SAMPLE_TEX2DARRAY_LOD(TEX, UV, LODN))

#endif // __LIGHTMAP_UTILS_INC__
