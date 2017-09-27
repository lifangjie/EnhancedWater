Shader "Custom/WaterSurfaceShader" {
	Properties {
		_Tess ("Tessellation", Range(1,32)) = 4
		_Transparency("Water transparency", Float) = 50.0
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
		_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		_WaterColor("Simple water color", COLOR) = (.172, .463, .435, 1)
		_HorizonColor("Simple water horizon color", COLOR) = (.172, .463, .435, 1)
		_Cubemap ("Environment Cubemap", Cube) = "_Skybox" {}
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "Blue" {}

		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		
		_GerstnerIntensity("Per vertex displacement", Float) = 1.0
		_GAmplitude ("Wave Amplitude", Vector) = (0.3 ,0.35, 0.25, 0.25)
		_GFrequency ("Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
		_GSteepness ("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GSpeed ("Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
		_GDirectionAB ("Wave Direction", Vector) = (0.3 ,0.85, 0.85, 0.25)
		_GDirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessDistance
		#pragma surface surf BlinnPhong fullforwardshadows vertex:vert tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6
		
		#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE
		#pragma multi_compile GERSTNER_ON GERSTNER_OFF

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		#define HAS_REFLECTION 1
		#endif
		#if defined (WATER_REFRACTIVE)
		#define HAS_REFRACTION 1
		#endif
		

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"
		#include "Tessellation.cginc"

		uniform fixed4 _WaveScale4;
		uniform fixed4 _WaveOffset;
		#if HAS_REFLECTION
		uniform float _ReflDistort;
		#endif
		//#if HAS_REFRACTION
		uniform float _RefrDistort;
		//#endif
		samplerCUBE _Cubemap;
		
		uniform fixed4 _WaterColor;

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
		sampler2D _ReflectionTex;
		#endif
		#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
		sampler2D _ReflectiveColor;
		#endif
		//#if defined (WATER_REFRACTIVE)
		uniform fixed _Transparency;
		//#endif
		#if defined (WATER_SIMPLE)
		uniform fixed4 _HorizonColor;
		#endif
		sampler2D _BumpMap;

		sampler2D _CameraDepthTexture;
		
		#if defined (GERSTNER_ON)
		#define WATER_VERTEX_DISPLACEMENT_ON
		#include "WaterInclude.cginc"
		uniform fixed4 _GAmplitude;
		uniform fixed4 _GFrequency;
		uniform fixed4 _GSteepness;
		uniform fixed4 _GSpeed;
		uniform fixed4 _GDirectionAB;
		uniform fixed4 _GDirectionCD;
		fixed _OffsetX, _OffsetZ, _Distance;
		fixed _WaveAmplitude;
		fixed _xImpact, _zImpact;
		#endif
		
		sampler2D _RefractionTex;

		struct Input {
		    float3 viewDir;
			float4 screenPos;
			float3 worldPos;
			float viewDepth;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		float _Tess;

		float4 tessDistance (appdata_full v0, appdata_full v1, appdata_full v2) {
			float minDist = 10.0;
			float maxDist = 25.0;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END
		
		void vert(inout appdata_full v)
		{
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			#if defined (GERSTNER_ON)
			fixed3 vtxForAni = (worldPos).xzz;

			fixed3 nrml;
			fixed3 offsets;
			Gerstner (
				offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
				_GAmplitude,												// amplitude
				_GFrequency,												// frequency
				_GSteepness,												// steepness
				_GSpeed,													// speed
				_GDirectionAB,												// direction # 1, 2
				_GDirectionCD												// direction # 3, 4
				);
			
			v.vertex.xyz += offsets;
			//o.worldNormal = nrml;
			v.normal = nrml;

			fixed offsetVert = v.vertex.x * v.vertex.x + v.vertex.z * v.vertex.z;
			fixed value = _WaveAmplitude  * 0.1 * sin(_Time.w * -2.5 + offsetVert * 0.4 + v.vertex.x * _OffsetX + v.vertex.z * _OffsetZ);
			if (sqrt(pow(worldPos.x - _xImpact, 2) + pow(worldPos.z - _zImpact, 2)) < _Distance) {
				v.vertex.y += value;
				v.normal += value;
			}
			#else
			v.normal = fixed3(0,1,0);
			#endif

			// Finish transforming the vetex by applying the projection matrix
			//o.pos = UnityObjectToClipPos(v.vertex);

			// scroll bump waves
			//o.bumpuv = o.worldPos.xzxz * _WaveScale4 + _WaveOffset;

			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
			//o.ref = ComputeScreenPos(o.pos);
			//TRANSFER_VERTEX_TO_FRAGMENT(o);
			IN.viewDepth = -UnityObjectToViewPos(v.vertex).z;
			#endif
			//o.projPos = o.ref;
			//return o;
		}

		void surf (Input IN, inout SurfaceOutput o) {
		    float4 bumpuv = IN.worldPos.xyxy * _WaveScale4 + _WaveOffset;
		    
		    float3 bump1 = UnpackNormal(tex2D(_BumpMap, bumpuv.xy)).rgb;
			float3 bump2 = UnpackNormal(tex2D(_BumpMap, bumpuv.wz)).rgb;
			float3 bump = normalize(bump1 + bump2);// + o.Normal;
			
			IN.screenPos.xy += bump.xy * _RefrDistort * IN.screenPos.z;
				
			// Albedo comes from a texture tinted by color
			float4 refrCol = tex2Dproj(_RefractionTex, IN.screenPos);
			fixed sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, IN.screenPos));
			fixed fade = saturate((sceneZ - IN.viewDepth) / _Transparency); 
			
			//fixed4 texColor = tex2D(_MainTex, i.uv.xy + speed);
			fixed3 reflDir = reflect(-IN.viewDir, o.Normal);
			fixed3 reflCol = texCUBE(_Cubemap, reflDir).rgb;
				
			fixed fresnel = pow(1 - saturate(dot(IN.viewDir, o.Normal)), 4);
			fixed3 finalColor = reflCol * fresnel + refrCol * (1 - fresnel);
				
			o.Albedo = refrCol;
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
