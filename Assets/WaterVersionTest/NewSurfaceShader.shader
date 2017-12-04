Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Tess ("TessDistance", Range(0,1000)) = 3
		minDist ("minDist", Range(0,2000)) = 3
		maxDist ("maxDist", Range(0,2000)) = 3
		PerlinNoise ("PerlinNoise Texture", 2D) = "white" {}
		DisplacementTexture ("Displacement Texture", 2D) = "white" {}
		[HideInInspector] PerlinOctave ("PerlinOctave", Vector) = (1.12, 0.59, 0.23)
		PerlinSize ("PerlinSize", float) = 1.0
		DisplacementSize ("DisplacementSize", float) = 1.0
		PerlinAmplitude ("PerlinAmplitude", Vector) = (35, 42, 57)
		[HideInInspector] PerlinGradient ("PerlinGradient", Vector) = (1.4, 1.6, 2.2)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0
		#include "Tessellation.cginc"
		sampler2D _MainTex;
			sampler2D PerlinNoise;
			sampler2D DisplacementTexture;
			float2 PerlinMovement;
			float3 PerlinOctave;
			float3 PerlinAmplitude;
			float3 PerlinGradient;
			float PerlinSize;
			float DisplacementSize;
		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END
				struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
				float _Tess;
			float minDist = 1;
			float maxDist = 500.0;
			float4 tessDistance (appdata_custom v0, appdata_custom v1, appdata_custom v2) {
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}	
			void vert (inout appdata_custom v)
			{
//				v2f o;

				float3 eye_vec = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				float blend_factor = (20000 - dist_2d) / (20000 - 800);
				blend_factor = clamp(blend_factor, 0, 1);

				// Add perlin noise to distant patches
				float perlin = 0;
				//blend_factor = 0;
				if (blend_factor < 1)
				{
					//float2 perlin_tc = uv_local * g_PerlinSize + g_UVBase;
					float2 uv = PerlinSize * v.texcoord.xy;
					float perlin_0 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.x + PerlinMovement, 0, 0)).w;
					float perlin_1 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.y + PerlinMovement, 0, 0)).w;
					float perlin_2 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.z + PerlinMovement, 0, 0)).w;
					//float perlin_0 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.x + PerlinMovement, 0).w;
					//float perlin_1 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.y + PerlinMovement, 0).w;
					//float perlin_2 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.z + PerlinMovement, 0).w;

					perlin = perlin_0 * PerlinAmplitude.x + perlin_1 * PerlinAmplitude.y + perlin_2 * PerlinAmplitude.z;
				}

				// Displacement map
				float3 displacement = 0;
				if (blend_factor > 0)
					displacement = tex2Dlod(DisplacementTexture, float4(v.texcoord.xy * DisplacementSize, 0, 0)).xzy;
				displacement = lerp(float3(0, perlin, 0), displacement, blend_factor);
				v.vertex.xyz += displacement;
				//v.vertex.xyz += float3(0, perlin, 0);

//				o.vertex = UnityObjectToClipPos(v.vertex);
//				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
//				return o;
			}
		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
