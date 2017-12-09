Shader "Custom/OceanSurface" {
	Properties {
		_Transparency("Water transparency", Float) = 50.0
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_BumpStrength ("BumpStrength", Range(0,10)) = 3
		_Smoothness ("Smoothness", Range(0,1)) = 0.89
		_DeepWaterColor ("Deep Water Color", Color) = (.0, .012, .05, 1)
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		[NoScaleOffset] _VerticesTex("Vertices Texture", 2D) = "Black" {}
		[NoScaleOffset] _RefractionTex("Internal Refraction", 2D) = "Black" {}
		[NoScaleOffset] _DepthTex("Internal Depth", 2D) = "Black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque"}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Custom noinstancing fullforwardshadows vertex:vert// tessellate:tessDistance
		//#pragma multi_compile_noinstancing
		//#pragma hull SubDToBezierHS

		#pragma target 5.0
		//#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"

		struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
//			float4 texcoord3 : TEXCOORD3;
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Specular;
			fixed3 Normal;
			half Emission;
			half Smoothness;
			half Occlusion; // occlusion (default 1)
			fixed Alpha;// alpha for transparencies
			fixed2 bump;
			half depth;
		};

//		inline half4 LightingCustom (SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
//		{
//			half4 c = 0;
//			return c;
//		}

		half _BumpStrength;
		half _Transparency;
		half _Smoothness;
		sampler2D _RefractionTex;
		sampler2D _DepthTex;
		half4 _DeepWaterColor;

		inline void LightingCustom_GI (
			SurfaceOutputCustom s,
			UnityGIInput data,
			inout UnityGI gi)
		{
		    fixed3 normal = normalize(fixed3(s.bump.x, _BumpStrength + 1, s.bump.y));
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
			gi = UnityGlobalIllumination(data, s.Occlusion, normal);
			#else
			Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, normal, s.Specular);
			gi = UnityGlobalIllumination(data, s.Occlusion, normal, g);
			#endif
		}

		uint _HeightMapSize;
		sampler2D _VerticesTex;
		sampler2D _NormalsTex;

		struct Input {
			float4 grabScreenPos;
			float viewZ;
			float4 bumpuv;
		};

		inline half4 LightingCustom_Deferred (SurfaceOutputCustom s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
		{
			half oneMinusReflectivity;
			//s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (1, 0.945, /*out*/ oneMinusReflectivity);
			
			fixed3 normal = normalize(fixed3(s.bump.x, _BumpStrength + 1, s.bump.y));
			//fixed3 normal = fixed3(0,1,0);

            //half grazingTerm = saturate(0.89 + (1-oneMinusReflectivity));
            //half fresnel = FresnelTerm (0.02, saturate(dot(s.Normal, viewDir)));
            half fresnel = (0.02 + 0.98 * Pow5(1 - saturate(dot(normal, viewDir))));// * s.depth;
    
			//half4 c = UNITY_BRDF_PBS ((1 - _Smoothness) * s.depth, 0.02, 0.98, 1, s.Normal, viewDir, gi.light, gi.indirect);

			UnityStandardData data;
			data.diffuseColor   = 1 - _Smoothness;
			data.occlusion      = 1;
			data.specularColor  = 0.02;
//			data.diffuseColor = 0;
//			data.occlusion      = 0;
//			data.specularColor  = 0;
			
            //half3 h = normalize (gi.light.dir + viewDir);
            //float nh = max (0, dot (s.Normal, h));
            //data.specularColor = pow (nh, 128.0) * 1;
			data.smoothness     = 1;
			data.normalWorld    = normal;

			UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

			//half4 emission = half4(s.Emission + c.rgb, 1);

			//return emission;
			s.Albedo = lerp(s.Albedo, _DeepWaterColor, s.depth);
			return half4(s.Albedo * (1 - fresnel), 1);
			//return half4(c.rgb, 1);
			//return 0;
			//return half4(s.depth, s.depth, s.depth,1);
			
		}
		
		sampler2D _BumpMap;
		uniform half4 _WaveScale4;
		uniform half4 _WaveOffset;
		void vert(inout appdata_custom v, out Input o)
		{
			//half4 uv = half4(v.texcoord.xy, 0, 0);
			//float2 slope = tex2Dlod(_NormalsTex, uv).xy * _BumpStrength;
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.grabScreenPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
			o.viewZ = -UnityObjectToViewPos(v.vertex).z;
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.bumpuv = worldPos.xzxz * _WaveScale4 + _WaveOffset;
		}

		void surf (Input IN, inout SurfaceOutputCustom o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.bump = UnpackNormal(tex2D(_BumpMap, IN.bumpuv.xy)).xy + UnpackNormal(tex2D(_BumpMap, IN.bumpuv.wz)).xy;
			half sceneZ = LinearEyeDepth (tex2Dproj(_DepthTex, UNITY_PROJ_COORD(IN.grabScreenPos)).r);
			half depth = saturate((sceneZ - IN.viewZ) / _Transparency);
			o.depth = depth;
			fixed3 refr = tex2Dproj(_RefractionTex, IN.grabScreenPos);
			o.Albedo = refr;
			// Metallic and smoothness come from slider variables
			o.Alpha = 1;
		}
		ENDCG
	}
	//FallBack "Diffuse"
}
