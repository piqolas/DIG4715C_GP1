Shader "Custom/VertexLit_PS1_Decal"
{
	Properties
	{
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		[MainColor] _Color ("Tint", Color) = (1, 1, 1, 1)
		_FalloffTex ("FallOff", 2D) = "white" {}
		[Toggle] _JITTER ("Vertex Jitter", Float) = 1
		_JitterGridScale ("Jitter Grid Scale", Range(0, 64)) = 16
		[Toggle] _AFFINE ("Affine Texture Mapping", Float) = 1
		[Toggle] _VERT_LIGHTMAPPING ("Vertex Shader Lightmapping", Float) = 1
		[Toggle] _DEBUG_OOR ("Debug: Out of Range", Float) = 0
	}
	SubShader
	{
		Tags { /*"RenderType" = "Opaque",*/ "Queue" = "Transparent+1" }
		// LOD 200

		Pass
		{
			// legacy vertex-lit pipeline
			Tags { "LightMode" = "Vertex" }

			ZTest LEqual
			// Cull Front
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			// ColorMask RGB
			// Blend DstColor SrcAlpha
			// Offset -1, -1

			CGPROGRAM

			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "PS1.cginc"

			#pragma shader_feature _JITTER_ON
			#pragma shader_feature _AFFINE_ON

			#pragma multi_compile_fog
			#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _FalloffTex;
			float4 _Color;

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos                    : SV_POSITION;
				float2 uv                     : TEXCOORD0;
				noperspective fixed3 col      : COLOR;
			#if USING_FOG
				fixed fog                     : TEXCOORD1;
			#endif
				float4 uvShadow               : TEXCOORD2;
				float4 uvFalloff              : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o = (v2f)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			#if USING_FOG
				float3 eyePos = UnityObjectToViewPos(v.vertex);
				float fogCoord = length(eyePos.xyz);
				UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
				//UNITY_CALC_FOG_FACTOR_RAW(v.fogCoord);
				o.fog = saturate(unityFogFactor);
			#endif

				// Jitter the clip-space projection
				o.pos = PS1ObjectToClipPos(v.vertex);

				// Pass UV
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			#if _AFFINE_ON
				o.uv *= o.pos.w;
			#endif

				o.uvShadow = mul(unity_Projector, v.vertex);
				o.uvFalloff = mul(unity_ProjectorClip, v.vertex);

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
				float2 uv = i.uv;
			#if _AFFINE_ON
				uv /= i.pos.w;
			#endif
				// 1) Sample & tint
				// float4 tex = tex2D(_MainTex, uv) * _Color;
				float4 tex = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uvShadow)) * _Color;
				// tex.a = 1.0 - tex.a;
				float4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				float4 lit = tex * texF.a;
				// float4 lit = lerp(float4(1.0, 1.0, 1.0, 0.0), tex, texF.a);

				// 2) Modulate by the affine-interpolated vertex color
				lit *= float4(i.col, 1.0);

				// 3) Apply fog
			#if USING_FOG
				lit.rgb = lerp(unity_FogColor.rgb, tex.rgb, i.fog);
				//UNITY_APPLY_FOG(i.fogCoord, lit);
			#endif

				return lit;
				// return float4(i.pos.w, 0.0, 0.0, 1.0);
			}

			ENDCG
		}

		Pass
		{
			// legacy vertex-lit pipeline
			Tags { "LightMode" = "VertexLM" }

			ZTest LEqual
			// Cull Front
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			// ColorMask RGB
			// Blend DstColor SrcAlpha
			// Offset -1, -1

			CGPROGRAM

			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "PS1.cginc"
			#include "PhotoshopBlendModes.cginc"
			#include "ColorspaceStuff.cginc"
			#include "LightmapUtils.cginc"

			#pragma shader_feature _JITTER_ON
			#pragma shader_feature _AFFINE_ON
			#pragma shader_feature _VERT_LIGHTMAPPING_ON
			#pragma shader_feature _DEBUG_OOR_ON

			#pragma multi_compile_fog
			#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 unity_Lightmap_ST;
			sampler2D _FalloffTex;
			float4 _Color;

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float3 uv     : TEXCOORD0;
				float3 uv1    : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos                    : SV_POSITION;
				float2 uv2                    : TEXCOORD0;
				float2 uv1                    : TEXCOORD1;
				float2 uv                     : TEXCOORD2;
				noperspective fixed3 col      : COLOR;
			#if USING_FOG
				UNITY_FOG_COORDS(3)
				// float fog                     : TEXCOORD3;
			#endif
				float4 uvShadow               : TEXCOORD4;
				float4 uvFalloff              : TEXCOORD5;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o = (v2f)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			#if USING_FOG
				// float3 eyePos = UnityObjectToViewPos(v.vertex);
				// float fogCoord = length(eyePos.xyz);
				// UNITY_CALC_FOG_FACTOR(fogCoord);

				// UNITY_CALC_FOG_FACTOR_RAW(v.fogCoord);

				// o.fog = saturate(unityFogFactor);

				UNITY_TRANSFER_FOG(o, o.pos);
			#endif

				// Jitter the clip-space projection
				o.pos = PS1ObjectToClipPos(v.vertex);

				// Pass UV
				o.uv2 = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				o.uv1 = v.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			#if _AFFINE_ON
				o.uv *= o.pos.w;
			#endif

				o.col = (fixed3)0;

			#if _VERT_LIGHTMAPPING_ON
				o.col = SampleAndDecodeLightmapLOD(unity_Lightmap, o.uv2.xy, 0.0);
			#endif

				o.uvShadow = mul(unity_Projector, v.vertex);
				o.uvFalloff = mul(unity_ProjectorClip, v.vertex);

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
				float2 uv = i.uv;
			#if _AFFINE_ON
				uv /= i.pos.w;
			#endif

				// Sample & tint
				// float4 tex = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;

				fixed4 tex = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uvShadow)) * _Color;
				fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 lightCol = tex * texF.a;

				// Capture the affine-interpolated vertex color
				// float3 lightCol = i.col;
				// Sample lightmap
			#if _VERT_LIGHTMAPPING_ON
				lightCol.rgb *= i.col;
			#else
				lightCol.rgb *= SampleAndDecodeLightmap(unity_Lightmap, i.uv2.xy);
			#endif

				// lit.rgb *= tex.rgb;
				//lit.rgb = HardLight(lit.rgb, bakedCol.rgb);
				//fixed4 lit = fixed4(HardLight(tex.rgb, lightmapCol.rgb), 1.0);
				fixed4 lit = fixed4(tex.rgb * lightCol.rgb, lightCol.a);

			#if _DEBUG_OOR_ON
				fixed3 clamped = saturate(tex);
				// diff will be non-zero where col <0 or >1
				bool3 diff = abs(tex.rgb - clamped.rgb) > 1.0f;
				// collapse to a single mask (0 = in range,  >0 = out of range)
				//float mask = max(max(diff.r, diff.g), diff.b);
				// highlight color:
				fixed3 highlight = diff ? fixed3(1.0, 1.0, 1.0) : tex.rgb;
				// lerp: when mask == 0 you get original; when mask > 0 you get highlight
				lit.rgb = highlight;
			#else

			// 3) Apply fog
			#if USING_FOG
				// lit.rgb = lerp(unity_FogColor.rgb, lit.rgb, i.fog);
				UNITY_APPLY_FOG_COLOR(i.fogCoord, lit, unity_FogColor.rgb);
				// UNITY_APPLY_FOG(i.fogCoord, lit);
			#endif

			#endif

				// return (tex < 0.5) ? 2.0 * tex * lightmap : 1.0 - 2.0 * (1.0 - tex) * (1.0 - lightmap);
				return lit;
				// return float4(i.pos.w, 0.0, 0.0, 1.0);
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}
