// #ifndef __PS1_INC__
// #define __PS1_INC__

#if _JITTER_ON

float _JitterGridScale;

// PS1 grid snap in clip space
#define JitterPos(v) floor(v * _JitterGridScale + 0.5) / _JitterGridScale;

// inline float3 JitterPos(float3 v)
// {
// 	return floor(v * _JitterGridScale + 0.5) / _JitterGridScale;
// }

#endif

inline float4 PS1ObjectToClipPos(float4 v)
{
	float4 clip = UnityObjectToClipPos(v);
#if _JITTER_ON
	clip.xyz = JitterPos(clip.xyz);
#endif

#if _AFFINE_ON
	float2 ndc = clip.xy / clip.w;
	float2 pix = (ndc * 0.5 + 0.5) * _ScreenParams.xy;
	pix = floor(pix);
	ndc = (pix / _ScreenParams.xy - 0.5) * 2.0;
	clip.xy = ndc * clip.w;
#endif

	return clip;
}

// #endif // __PS1_INC__
