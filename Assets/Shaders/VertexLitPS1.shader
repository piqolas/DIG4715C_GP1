Shader "Custom/VertexLit_PS1"
{
	Properties
	{
		[MainTexture] _MainTex ("Texture", 2D) = "white" {}
		[MainColor] _Color ("Tint", Color) = (1, 1, 1, 1)
		[Toggle] _JITTER ("Vertex Jitter", Float) = 1
		_JitterGridScale ("Jitter Grid Scale", Range(0, 64)) = 16
		[Toggle] _AFFINE ("Affine Texture Mapping", Float) = 1
		[Toggle] _DEBUG_OOR ("Debug: Out of Range", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		// LOD 200

		Pass
		{
			// legacy vertex-lit pipeline
			Tags { "LightMode" = "Vertex" }

			ZTest LEqual
			Cull Off
			ZWrite On

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
			float4 _Color;

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
				noperspective float3 col      : COLOR;
			#if USING_FOG
				fixed fog                     : TEXCOORD1;
			#endif
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
				float4 tex = tex2D(_MainTex, uv) * _Color;
				// 2) Modulate by the affine-interpolated vertex color
				float4 lit = tex * fixed4(i.col, 1.0);
				// 3) Apply fog
			#if USING_FOG
				lit.rgb = lerp(unity_FogColor.rgb, lit.rgb, i.fog);
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
			Cull Off
			ZWrite On

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
			#pragma shader_feature _DEBUG_OOR_ON

			#pragma multi_compile_fog
			#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

			UNITY_DECLARE_TEX2D(_MainTex);
			float4 _MainTex_ST;
			float4 unity_Lightmap_ST;
			float4 _Color;

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
				noperspective float3 col      : COLOR;
			#if USING_FOG
				fixed fog                     : TEXCOORD3;
			#endif
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
				o.uv2 = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				o.uv1 = v.uv1.xy * unity_Lightmap_ST.xy + unity_Lightmap_ST.zw;
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
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

			float4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv;
			#if _AFFINE_ON
				uv /= i.pos.w;
			#endif

				// Sample & tint
				float4 tex = UNITY_SAMPLE_TEX2D(_MainTex, uv) * _Color;

				// Capture the affine-interpolated vertex color
				float3 vertCol = i.col;
				// Sample lightmap
				float3 lightmapCol = SampleAndDecodeLightmap(unity_Lightmap, i.uv2.xy);
				// Combine light colors
				float3 lightCol = vertCol + lightmapCol;

				// lit.rgb *= tex.rgb;
				//lit.rgb = HardLight(lit.rgb, bakedCol.rgb);
				//fixed4 lit = fixed4(HardLight(tex.rgb, lightmapCol.rgb), 1.0);
				float4 lit = float4(tex.rgb * lightCol.rgb, 1.0);

			#if _DEBUG_OOR_ON
				float3 clamped = saturate(tex);
				// diff will be non-zero where col <0 or >1
				bool3 diff = abs(tex.rgb - clamped.rgb) > 1.0f;
				// collapse to a single mask (0 = in range,  >0 = out of range)
				//float mask = max(max(diff.r, diff.g), diff.b);
				// highlight color:
				float3 highlight = diff ? fixed3(1.0, 1.0, 1.0) : tex.rgb;
				// lerp: when mask == 0 you get original; when mask > 0 you get highlight
				lit.rgb = highlight;
			#else

				// 3) Apply fog
				#if USING_FOG
					lit.rgb = lerp(unity_FogColor.rgb, lit.rgb, i.fog);
					//UNITY_APPLY_FOG(i.fogCoord, lit);
				#endif

			#endif

				// return (tex < 0.5) ? 2.0 * tex * lightmap : 1.0 - 2.0 * (1.0 - tex) * (1.0 - lightmap);
				return lit;
				// return float4(i.pos.w, 0.0, 0.0, 1.0);
			}

			ENDCG
		}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders

			#include "UnityCG.cginc"

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}

	Fallback "Diffuse"
}
