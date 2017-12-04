// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tessellation/Vector Displacement Quad" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_SpecMap ("Specmap", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_DispTex ("Disp Texture", 2D) = "gray" {}
		_TessEdge ("Edge Tess", Range(1,25)) = 2
		_Displacement ("Displacement", Range(0, 1)) = 0.1
		_fadeDist ("Start Distance", Range(0, 15)) = 0.1
	}
	SubShader {
		Pass {
			Tags {"LightMode" = "ForwardBase"}
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain
 
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdbase
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
 
			float _TessEdge;
			float _Displacement;
			float _distFalloff;
			float _fadeDist;
			fixed4 _Color;
			half _Shininess;
 
			sampler2D _MainTex;
			SamplerState	sampler_MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _BumpMap_ST;
 
			Texture2D _DispTex;
			SamplerState	sampler_DispTex;
			//Texture2D _BumpMap;
			sampler2D _BumpMap;
			SamplerState	sampler_BumpMap;
			Texture2D _SpecMap;
			SamplerState	sampler_SpecMap;
 
			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
			};
 
			void surf (Input IN, inout SurfaceOutput o) {
				fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = tex.rgb * _Color.rgb;
				o.Gloss = tex.a;
				o.Alpha = tex.a * _Color.a;
				o.Specular = _Shininess;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			}
			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct tess_appdata {
				float4 vertex : POS;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct PS_INPUT
			{
				float4 pos   : POSITION;
				float3 normal     : NORMAL;
				float4 tangent     : TANGENT;
				float4 uv   : TEXCOORD;
				float3 lightDir : TEXCOORD1;
				float3 viewDir : COLOR;
				fixed3 vlight : TEXCOORD2;
				LIGHTING_COORDS(3,4)
			};
			struct PS_RenderOutput{
				float4 f4Color      : SV_Target0;
			};
			struct HS_CONSTANT_OUTPUT
			{
				float edges[4]  : SV_TessFactor;
				float inside[2] : SV_InsideTessFactor;
			};
 
			struct HS_OUTPUT
			{
				float3 pos  : POS;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD;
				float4 tangent: TANGENT;
				float3 lightDir : TEXTCOORD1;
				float3 viewDir : COLOR;
				fixed3 vlight : TEXCOORD2;
			};
 
			void vert(inout appdata v){
				v.vertex = mul(UNITY_MATRIX_MV, v.vertex);
			}
			HS_CONSTANT_OUTPUT HSConstant( InputPatch<appdata, 4> ip )
			{
				HS_CONSTANT_OUTPUT output;
 
				output.edges[0] = _TessEdge / ((-ip[0].vertex.z - _ProjectionParams.y)/_fadeDist);
				output.edges[1] = _TessEdge / ((-ip[1].vertex.z - _ProjectionParams.y)/_fadeDist);
				output.edges[2] = _TessEdge / ((-ip[2].vertex.z - _ProjectionParams.y)/_fadeDist);
				output.edges[3] = _TessEdge / ((-ip[3].vertex.z - _ProjectionParams.y)/_fadeDist);
 
				output.inside[0] = _TessEdge / ((-ip[0].vertex.z - _ProjectionParams.y)/_fadeDist);
				output.inside[1] = _TessEdge / ((-ip[0].vertex.z - _ProjectionParams.y)/_fadeDist);
 
				return output;
			}
 
			[domain("tri")]
			[partitioning("integer")]
			[outputtopology("triangle_cw")]
			[outputcontrolpoints(4)]
			[patchconstantfunc("HSConstant")]
			appdata hull( InputPatch<appdata, 4> ip, uint cpid : SV_OutputControlPointID, uint pid : SV_PrimitiveID )
			{
				return ip[cpid];
			}
 
			[domain("quad")]
			PS_INPUT domain( HS_CONSTANT_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<tess_appdata, 4> patch )
			{
				PS_INPUT o;
 
				float3 topMidpoint = lerp(patch[0].vertex, patch[1].vertex, UV.x);
				float3 bottomMidpoint = lerp(patch[3].vertex, patch[2].vertex, UV.x);
 
				float3 pos = lerp(topMidpoint, bottomMidpoint, UV.y);
 
				float2 uvtopMidpoint = lerp(patch[0].texcoord, patch[1].texcoord, UV.x);
				float2 uvbottomMidpoint = lerp(patch[3].texcoord, patch[2].texcoord, UV.x);
 
				o.uv.xy = lerp(uvtopMidpoint, uvbottomMidpoint, UV.y);
 
				topMidpoint = lerp(patch[0].normal, patch[1].normal, UV.x);
				bottomMidpoint = lerp(patch[3].normal, patch[2].normal, UV.x);
 
				//float4 pNormal = _BumpMap.Sample( sampler_BumpMap, input.uv );
 
				float3 normal = lerp(topMidpoint, bottomMidpoint, UV.y);
				o.normal= normal;// unity_LightColor[0].rgb * max( 0, dot( normal, unity_LightPosition[0].xyz ) ) + UNITY_LIGHTMODEL_AMBIENT.rgb;
 
				//pos = mul(UNITY_MATRIX_MV, pos);
				float3 disp = _DispTex.SampleLevel (sampler_DispTex, o.uv, 0).rgb * _Displacement;
				pos += normal * disp;
 
				o.pos = mul (UNITY_MATRIX_P, float4(pos, 1));
 
				float4 tangenttopMidpoint = lerp(patch[0].tangent, patch[1].tangent, UV.x);
				float4 tangentbottomMidpoint = lerp(patch[3].tangent, patch[2].tangent, UV.x);
 
				o.tangent = lerp(tangentbottomMidpoint, tangenttopMidpoint, UV.y);
				appdata v;
				v.normal = o.normal;
				v.tangent = o.tangent;
				TANGENT_SPACE_ROTATION;
				// To view space
				o.uv.xy = TRANSFORM_TEX(o.uv.xy, _MainTex);
				o.uv.zw = TRANSFORM_TEX(o.uv.xy, _BumpMap);
				o.lightDir = mul(rotation, ObjSpaceLightDir(o.pos));
				o.viewDir = mul(rotation, ObjSpaceViewDir(o.pos));
				float3 worldN = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
				o.vlight = ShadeSH9(float4(worldN,1.0));
				float3 worldPos = mul(unity_ObjectToWorld, o.pos).xyz;
				  o.vlight += Shade4PointLights (
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, worldPos, worldN );
				// //input.uv = I.uv;
				o.tangent = o.tangent;
				// o.uv = v.uv;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
 
				return o;    
			}
 
			fixed4 frag( PS_INPUT input) : COLOR{
					Input surfIN;
				surfIN.uv_MainTex = input.uv.xy;
				surfIN.uv_BumpMap = input.uv.zw;
				#ifdef UNITY_COMPILER_HLSL
				SurfaceOutput o = (SurfaceOutput)0;
				#else
				SurfaceOutput o;
				#endif
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Specular = 0.0;
				o.Alpha = 0.0;
				o.Gloss = 0.0;
				surf (surfIN, o);
				//o.Albedo = tex2D(_MainTex, input.uv);
				  fixed atten = LIGHT_ATTENUATION(input);
				  fixed4 c = 0;
				  //c = LightingBlinnPhong (o, input.lightDir, normalize(half3(input.viewDir)), atten);
				  //c = LightingLambert (o, input.lightDir, atten);
				  c.rgb += o.Albedo * input.vlight;
				  c.a = o.Alpha;
				  return c;
			}
 
			ENDCG
		}
 
	}
}