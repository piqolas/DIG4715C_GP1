Shader "Custom/VertexLit_PS1"
{
	Properties
	{
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		[MainColor] _Color ("Tint", Color) = (1, 1, 1, 1)
		[Toggle] _JITTER ("Vertex Jitter", Float) = 1
		_JitterGridScale ("Jitter Grid Scale", Range(0, 64)) = 16
		[Toggle] _AFFINE ("Affine Texture Mapping", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		// LOD 200

		Pass
		{
			// legacy vertex-lit pipeline
			Tags { "LightMode" = "Vertex" }

			CGPROGRAM

			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma shader_feature _JITTER_ON
			#pragma shader_feature _AFFINE_ON

			#pragma multi_compile_fog
			#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

		#if _JITTER_ON
			float _JitterGridScale;

			// PS1 grid snap in clip space
			inline float3 JitterPos(float3 v)
			{
				return floor(v * _JitterGridScale + 0.5) / _JitterGridScale;
			}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv     : TEXCOORD0;
				// UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos                    : SV_POSITION;
				float2 uv                     : TEXCOORD0;
				noperspective float3 col      : COLOR;
			#if USING_FOG
				fixed fog                     : TEXCOORD1;
			#endif
				// UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o = (v2f)0;

				// UNITY_SETUP_INSTANCE_ID(v);
				// UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			#if USING_FOG
				float3 eyePos = UnityObjectToViewPos(v.vertex);
				float fogCoord = length(eyePos.xyz);
				UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
				//UNITY_CALC_FOG_FACTOR_RAW(v.fogCoord);
				o.fog = saturate(unityFogFactor);
			#endif

				// Jitter the projection
				o.pos = PS1ObjectToClipPos(v.vertex);

				// Pass UV
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			#if _AFFINE_ON
				o.uv *= o.pos.w;
			#endif

				// Compute per-vertex lighting
				//	ShadeVertexLights does up to 4 real-time vertex lights + ambient
				o.col = ShadeVertexLights(v.vertex, v.normal);

				// Do fog (this is how it appears to be done in newer pipelines;
				// leaving it here in case I need to remember where it goes later)
			// #if USING_FOG
			// 	UNITY_TRANSFER_FOG(o, o.pos);
			// #endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
			#if _AFFINE_ON
				float2 uv = i.uv / i.pos.w;
			#else
				float2 uv = i.uv;
			#endif
				// 1) Sample & tint
				float4 tex = tex2D(_MainTex, uv) * _Color;
				// 2) Modulate by the affine-interpolated vertex color
				float4 lit = tex * fixed4(i.col, 1.0);
				// 3) Apply fog
			#if USING_FOG
				lit.rgb = lerp(unity_FogColor.rgb, lit.rgb, i.fog);
				//UNITY_APPLY_FOG(i.fogCoord, lit);
			#endif

				return lit;
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}
