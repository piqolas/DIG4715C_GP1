Shader "Custom/Screenspace/BlitBlueNoiseDither"
{
	Properties
	{
		[MainTexture] _MainTex ("Source (RGB)", 2D) = "white" {}
		_NoiseTex ("Blue Noise Tex", 2D) = "white" {}
		[Toggle] _DITHER ("Dithering", Float) = 1
		_DitherTune ("Dither Tune", Range(-64, 64)) = 0
		[Toggle] _QUANTIZE ("Color Quantization", Float) = 1
		_ColorDepth ("Color Depth (bits)", Range(1, 8)) = 6
		[Toggle] _USEEGAPAL ("Use EGA Palette", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma shader_feature _DITHER_ON
			#pragma shader_feature _QUANTIZE_ON
			#pragma shader_feature _USEEGAPAL_ON

			// #define DITHER_OR_QUANT (defined(_DITHER_ON) || defined(_QUANTIZE_ON))

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float4 _NoiseTex_TexelSize;

		#if _DITHER_ON
			float _DitherTune;
		#endif
		#if (defined(_DITHER_ON) || defined(_QUANTIZE_ON)) // DITHER_OR_QUANT
			float _ColorDepth;
		#endif

		#if _DITHER_ON
			// Blue-Noise Dither
			inline float3 DitherPattern(float3 col, float2 uv)
			{
				// Calculate tiling based on texture dimensions
				// _NoiseTex_TexelSize.xy contains (1/W, 1/H)
				// _MainTex_TexelSize.xy contains (1/W, 1/H)
				float2 autoTiling = _MainTex_TexelSize.xy / _NoiseTex_TexelSize.xy;

				// Apply tiling
				float2 noiseUV = uv / autoTiling + _NoiseTex_ST.zw;
				// Fix blue noise aspect ratio
				noiseUV *= _ScreenParams.x / _ScreenParams.y;

				// Sample noise
				float3 noise = tex2D(_NoiseTex, noiseUV).rgb;

				// compute threshold:
				// multiplier = 2^(8 - bits) – 1
				float mult = pow(2.0, 8.0 - _ColorDepth) - 1.0;

				// center noise around zero and scale
				float3 thresh = (noise - 0.5) * ((mult + _DitherTune) / 255.0);

				return col + thresh; // noise;
			}
		#endif

		#if _QUANTIZE_ON
			// Quantize to N-bit per channel
			inline float3 ColorDepthReduction(float3 col)
			{
				float levels = pow(2.0, _ColorDepth) - 1.0;
				col *= levels;
				col = floor(col) + step(0.5, frac(col));

				return col / levels;
			}
		#endif

		#if _USEEGAPAL_ON
			// (Optional) Optimized EGA palette remapping
			// We quantize each channel to 4 levels, then remap via a small switch-case.

			// Precomputed divider for 4-level quantization
			static const float DIV3 = 1.0 / 3.0;

			// Remap quantized RGB to nearest EGA color
			float3 EGAPalette(float3 col)
			{
				// Quantize to 0..3 with mid-point rounding
				float3 q = floor(col * 3.0 + 0.5);

				int r = (int)q.r;
				int g = (int)q.g;
				int b = (int)q.b;

				// Flatten index: r*16 + g*4 + b
				int idx = (r << 4) | (g << 2) | b;

				// Default: simple quantized color
				float3 outC = q * DIV3;

				// 4) Override only the specific EGA entries
				switch (idx)
				{
					// Bright green (1,3,1) and related
					case 12: case 14: case 44: case 13:
					case 28: case 29: case 45: case 46:
						outC = float3(1, 3, 1) * DIV3; break;

					// Green (0,2,0)
					case 4: case 5: case 8: case 9:
					case 24: case 25:
						outC = float3(0, 2, 0) * DIV3; break;

					// Bright red (3,1,1)
					case 48: case 49: case 52: case 53:
						outC = float3(3, 1, 1) * DIV3; break;

					// Red (2,0,0)
					case 16: case 32: case 33:
						outC = float3(2, 0, 0) * DIV3; break;

					// Bright cyan (1,3,3)
					case 15: case 31: case 47:
						outC = float3(1, 3, 3) * DIV3; break;

					// Cyan (0,2,2)
					case 10: case 26:
						outC = float3(0, 2, 2) * DIV3; break;

					// Bright blue (1,1,3)
					case 3: case 11: case 27:
						outC = float3(1, 1, 3) * DIV3; break;

					// Blue (0,0,2)
					case 1: case 2: case 6: case 7: case 23:
						outC = float3(0, 0, 2) * DIV3; break;

					// Brown (2,1,0)
					case 20: case 36: case 37:
						outC = float3(2, 1, 0) * DIV3; break;

					// Bright yellow (3,3,1)
					case 40: case 41: case 60: case 61: case 62:
						outC = float3(3, 3, 1) * DIV3; break;

					// Magenta (2,0,2)
					case 34: case 35: case 38: case 39:
						outC = float3(2, 0, 2) * DIV3; break;

					// Bright magenta (3,1,3)
					case 50: case 51: case 54: case 55: case 59:
						outC = float3(3, 1, 3) * DIV3; break;
				}

				return outC;
			}
		#endif

			float4 frag(v2f_img i) : SV_Target
			{
				// sample original
				float2 uv = i.uv;
				float4 col = tex2D(_MainTex, uv);

			#if _DITHER_ON
				// apply blue-noise dither
				col = float4(DitherPattern(col.rgb, uv), col.a);
			#endif

			#if _QUANTIZE_ON
				// reduce to N-bit
				col = float4(ColorDepthReduction(col.rgb), col.a);
			#endif

			#if _USEEGAPAL_ON
				// (optionally) remap to EGA palette
				col.rgb = EGAPalette(col).rgb;
			#endif

				return col;
			}

			ENDCG
		}
	}
}
