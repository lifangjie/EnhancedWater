Shader "Custom/OceanSurface" {
	Properties {
		_Transparency("Water transparency", Float) = 50.0
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_Tess ("TessDistance", Range(0,10)) = 3
		_BumpStrength ("BumpStrength", Range(0,10)) = 3
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		[NoScaleOffset] _VerticesTex("Vertices Texture", 2D) = "Black" {}
		[NoScaleOffset] _RefractionTex("Internal Refraction", 2D) = "Blue" {}
		[NoScaleOffset] _DepthTex("Internal Depth", 2D) = "Blue" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Custom fullforwardshadows vertex:vert// tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6
		#include "Tessellation.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"

		struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Specular;
			fixed3 Normal;  // tangent space normal, if written
			half3 Emission;
			half Smoothness;
			half Occlusion; // occlusion (default 1)
			fixed Alpha;// alpha for transparencies
		};

		inline half4 LightingCustom (SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
		{
			half4 c = 1;
			return c;
		}

		inline half4 LightingCustom_Deferred (SurfaceOutputCustom s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
		{
			// energy conservation
			half oneMinusReflectivity;
			//s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

			half fresnel = pow(1 - saturate(dot(viewDir, float3(0,1,0))), 4);
			UnityStandardData data;
			data.diffuseColor   = 0.1;
			data.occlusion  = fresnel * s.Occlusion;
			data.specularColor  = 1;
			data.smoothness = 1;
			data.normalWorld= s.Normal;

			UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

			return half4(s.Albedo, 1);
			//return half4(0.002,0.01,0.012, 1);
		}

		inline void LightingCustom_GI (
			SurfaceOutputCustom s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
			Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
			#endif
		}
		// half4 Lighting<Name> (SurfaceOutput s, UnityGI gi);
		// half4 Lighting<Name> (SurfaceOutput s, half3 viewDir, UnityGI gi);
		// half4 Lighting<Name>_Deferred (SurfaceOutput s, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal);
		// half4 Lighting<Name>_PrePass (SurfaceOutput s, half4 light);

		float _Tess;
		uint _HeightMapSize;
		sampler2D _VerticesTex;
		sampler2D _NormalsTex;


		float4 tessDistance (appdata_custom v0, appdata_custom v1, appdata_custom v2) {
			float minDist = 10.0;
			float maxDist = 25.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}

		struct Input {
			float4 grabScreenPos;
			float viewZ;
		};

		half _Glossiness;
		half _Metallic;
		half _BumpStrength;
		half _Transparency;
		sampler2D _RefractionTex;
		sampler2D _DepthTex;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		//UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		//UNITY_INSTANCING_CBUFFER_END

		float3 generateNormal(float2 positon, sampler2D heightMap, float heightMapSize) {
			float l = tex2Dlod(heightMap,float4(positon.x-1.0/heightMapSize,positon.y, 0, 0)).y;
			float r = tex2Dlod(heightMap,float4(positon.x+1.0/heightMapSize,positon.y, 0, 0)).y;
			float d = tex2Dlod(heightMap,float4(positon.x,positon.y-1.0/heightMapSize, 0, 0)).y;
			float u = tex2Dlod(heightMap,float4(positon.x,positon.y+1.0/heightMapSize, 0, 0)).y;
			float3 normal = float3(l-r, 2, d-u);
			return normalize(normal);
		}

		void vert(inout appdata_custom v, out Input o)
		{
			//float id = vid + 0.5;
			//half4 uv = half4(id / 4096, v.uv.y + (_Time.y/16), 0 , 0);//%256) / 4096, 0 ,0);
			half4 uv = half4(v.texcoord.xy, 0, 0);

			//uint sign = (vid/250 + vid%250) & 1;
			//half2 signs = half2(1, -1);
			v.vertex.xyz += tex2Dlod(_VerticesTex, uv).xyz;// * signs[sign];
			float2 slope = tex2Dlod(_NormalsTex, uv).xy * _BumpStrength;
			v.normal = normalize(float3(-slope.x, 0.5f, -slope.y));
			//v.normal = generateNormal(v.texcoord.xy, _VerticesTex, _HeightMapSize);
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.grabScreenPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
			o.viewZ = -UnityObjectToViewPos(v.vertex).z;
		}

		void surf (Input IN, inout SurfaceOutputCustom o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			half3 refr = tex2Dproj(_RefractionTex, IN.grabScreenPos);
			half sceneZ = LinearEyeDepth (tex2Dproj(_DepthTex, UNITY_PROJ_COORD(IN.grabScreenPos)).r);
			half depth = saturate((sceneZ - IN.viewZ) / _Transparency);
			o.Albedo = refr;
			// Metallic and smoothness come from slider variables
			o.Occlusion = depth;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
