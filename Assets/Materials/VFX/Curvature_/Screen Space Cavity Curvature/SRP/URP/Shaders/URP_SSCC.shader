Shader "Hidden/Universal Render Pipeline/SSCC"
{
    Properties
    {
        _MainTex("", any) = "" {}
    }

    HLSLINCLUDE

    #pragma target 3.0
    #pragma prefer_hlslcc gles
    #pragma exclude_renderers d3d11_9x
    #pragma editor_sync_compilation

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
	#if VERSION_GREATER_EQUAL(10, 0)
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
	#endif

    TEXTURE2D_X(_MainTex);
    SAMPLER(sampler_LinearClamp);
    SAMPLER(sampler_PointClamp);

    CBUFFER_START(FrequentlyUpdatedUniforms)
    float4 _Input_TexelSize;
    float4 _UVToView;
    float4x4 _WorldToCameraMatrix;

	float _EffectIntensity;
	float _DistanceFade;
	
	float _CurvaturePixelRadius;
	float _CurvatureBrights;
	float _CurvatureDarks;

	float _CavityWorldRadius;
	float _CavityBrights;
	float _CavityDarks;
    CBUFFER_END

    struct Attributes
    {
        float4 positionOS   : POSITION;
        float2 uv           : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS    : SV_POSITION;
        float2 uv            : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = float4(input.positionOS.xyz, 1.0);
        #if UNITY_UV_STARTS_AT_TOP
        output.positionCS.y *= -1;
        #endif
        output.uv = input.uv;
        return output;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always Blend Off Cull Off

        Pass // 0
        {
            Name "Copy"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
                return SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv);
            }
            ENDHLSL
        }

        Pass // 1
        {
            Name "Composite"

            ColorMask RGB

            HLSLPROGRAM
			#pragma multi_compile_local __ DEBUG_EFFECT DEBUG_NORMALS
			#pragma multi_compile_local __ ORTHOGRAPHIC_PROJECTION
			#pragma multi_compile_local __ NORMALS_RECONSTRUCT
			#pragma multi_compile_fragment __ _GBUFFER_NORMALS_OCT
			#pragma multi_compile_local CAVITY_SAMPLES_6 CAVITY_SAMPLES_8 CAVITY_SAMPLES_12
			#pragma multi_compile_local __ SATURATE_CAVITY
			#pragma multi_compile_local __ OUTPUT_TO_TEXTURE

            #pragma vertex Vert
            #pragma fragment Composite_Frag

			//
			inline half4 FetchSceneColor(float2 uv) {
				return SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, uv);
			}
			inline float FetchRawDepth(float2 uv) {
				return SampleSceneDepth(uv);
			}
			inline float LinearizeDepth(float depth) {
				#if ORTHOGRAPHIC_PROJECTION
					#if UNITY_REVERSED_Z
						depth = 1 - depth;
					#endif
					float linearDepth = _ProjectionParams.y + depth * (_ProjectionParams.z - _ProjectionParams.y);
				#else
					float linearDepth = LinearEyeDepth(depth, _ZBufferParams);
				#endif
				return linearDepth;
			}
			inline float3 FetchViewPos(float2 uv) {
				float depth = LinearizeDepth(FetchRawDepth(uv));
				//return float3((uv * _UVToView.xy + _UVToView.zw) * depth, depth);
				float4 UVToView = float4(2 / unity_CameraProjection._m00, -2 / unity_CameraProjection._m11, -1 / unity_CameraProjection._m00, 1 / unity_CameraProjection._m11);
				#if ORTHOGRAPHIC_PROJECTION
					return float3((uv * UVToView.xy + UVToView.zw), depth);
				#else
					return float3((uv * UVToView.xy + UVToView.zw) * depth, depth);
				#endif
			}
			inline float3 MinDiff(float3 P, float3 Pr, float3 Pl) {
				float3 V1 = Pr - P;
				float3 V2 = P - Pl;
				return (dot(V1, V1) < dot(V2, V2)) ? V1 : V2;
			}
			inline float3 FetchViewNormals(float3 P, float2 uv) {
				#if NORMALS_RECONSTRUCT
					float c = FetchRawDepth(uv);
					half3 viewSpacePos_c = FetchViewPos(uv);
					// get data at 1 pixel offsets in each major direction
					half3 viewSpacePos_l = FetchViewPos(uv + float2(-1.0, 0.0) * _Input_TexelSize.xy);
					half3 viewSpacePos_r = FetchViewPos(uv + float2(+1.0, 0.0) * _Input_TexelSize.xy);
					half3 viewSpacePos_d = FetchViewPos(uv + float2(0.0, -1.0) * _Input_TexelSize.xy);
					half3 viewSpacePos_u = FetchViewPos(uv + float2(0.0, +1.0) * _Input_TexelSize.xy);
					half3 l = viewSpacePos_c - viewSpacePos_l;
					half3 r = viewSpacePos_r - viewSpacePos_c;
					half3 d = viewSpacePos_c - viewSpacePos_d;
					half3 u = viewSpacePos_u - viewSpacePos_c;
					half4 H = half4(
						FetchRawDepth(uv + float2(-1.0, 0.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(+1.0, 0.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(-2.0, 0.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(+2.0, 0.0) * _Input_TexelSize.xy)
					);
					half4 V = half4(
						FetchRawDepth(uv + float2(0.0, -1.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(0.0, +1.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(0.0, -2.0) * _Input_TexelSize.xy),
						FetchRawDepth(uv + float2(0.0, +2.0) * _Input_TexelSize.xy)
					);
					half2 he = abs((2 * H.xy - H.zw) - c);
					half2 ve = abs((2 * V.xy - V.zw) - c);
					half3 hDeriv = he.x < he.y ? l : r;
					half3 vDeriv = ve.x < ve.y ? d : u;
					float3 N = normalize(cross(hDeriv, vDeriv));
				#else
					#if VERSION_GREATER_EQUAL(10, 0)
						float3 N = SampleSceneNormals(uv);
						#if UNITY_VERSION >= 202110 || VERSION_GREATER_EQUAL(10, 9)
							N = mul((float3x3)_WorldToCameraMatrix, N);
						#else
							N = float3(N.x, N.y, -N.z); //?
						#endif
					#else
						float3 N = float3(0, 0, 0);
					#endif
					N = float3(N.x, -N.yz);
				#endif

				N = float3(N.x, -N.y, N.z);
				return N;
			}
			//
			#include "../../../Shaders/Shared.cginc"

            ENDHLSL
        }
    }

    Fallback Off
}
