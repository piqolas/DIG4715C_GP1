#ifndef __COLORSPACE_STUFF_INC__
#define __COLORSPACE_STUFF_INC__

float4 toLinearFromSRGB(float4 sRGB)
{
	float4 higher = pow((sRGB + 0.055) / 1.055, 2.4);
	float4 lower = sRGB / 12.92;

	bool4 cutoff = sRGB < float4(0.04045, 0.04045, 0.04045, 0.04045);

	return cutoff ? lower : higher;
}

float3 toLinearFromSRGB(float3 sRGB)
{
	return toLinearFromSRGB(float4(sRGB, 1.0));
}

float4 fromLinearToSRGB(float4 linearRGB)
{
	float4 higher = 1.055 * pow(linearRGB, 1.0 / 2.4) - 0.055;
	float4 lower = linearRGB * 12.92;

	// per‐component comparison yields a bool4,
	// and HLSL applies the ternary per component
	bool4 cutoff = linearRGB < float4(0.0031308, 0.0031308, 0.0031308, 0.0031308);

	return cutoff ? lower : higher;
}

#endif // __COLORSPACE_STUFF_INC__
