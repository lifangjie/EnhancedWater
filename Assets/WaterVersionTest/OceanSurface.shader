Shader "Custom/OceanSurface" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_Tess ("TessDistance", Range(0,10)) = 3
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		[NoScaleOffset] _VerticesTex("Vertices Texture", 2D) = "Black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows tessellate:tessDistance vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityCG.cginc"

		struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		// half4 Lighting<Name> (SurfaceOutput s, UnityGI gi);
		// half4 Lighting<Name> (SurfaceOutput s, half3 viewDir, UnityGI gi);
		// half4 Lighting<Name>_Deferred (SurfaceOutput s, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal);
		// half4 Lighting<Name>_PrePass (SurfaceOutput s, half4 light);

		sampler2D _MainTex;
		float _Tess;


		float4 tessDistance (appdata_custom v0, appdata_custom v1, appdata_custom v2) {
			float minDist = 10.0;
			float maxDist = 25.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _VerticesTex;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		//UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		//UNITY_INSTANCING_CBUFFER_END
		float3 generateNormal(float2 positon, sampler2D heightMap, float heightMapSize) {
			float t = tex2Dlod(heightMap,float4(positon.x,positon.y+1.0/heightMapSize, 0, 0)).y;
			float b = tex2Dlod(heightMap,float4(positon.x,positon.y-1.0/heightMapSize, 0, 0)).y;
			float l = tex2Dlod(heightMap,float4(positon.x+1.0/heightMapSize,positon.y, 0, 0)).y;
			float r = tex2Dlod(heightMap,float4(positon.x-1.0/heightMapSize,positon.y, 0, 0)).y;
			float3 tanZ = float3(0.0f, t-b, 1.0f);
			float3 tanX = float3(1.0f, r-l, 0.0f);
			float3 normal = cross(tanZ, tanX) + float3(0,1,0);
			return normalize(normal);
		}

		void vert(inout appdata_custom v)
		{
			//float id = vid + 0.5;
			//half4 uv = half4(id / 4096, v.uv.y + (_Time.y/16), 0 , 0);//%256) / 4096, 0 ,0);
			half4 uv = half4(v.texcoord.xy, 0, 0);

			//uint sign = (vid/250 + vid%250) & 1;
			//half2 signs = half2(1, -1);
			v.vertex.xyz += tex2Dlod(_VerticesTex, uv).xyz;// * signs[sign];
			//v.normal = tex2Dlod(_NormalsTex, uv).xyz;
			v.normal = generateNormal(v.texcoord.xy, _VerticesTex, 512);
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
