Shader "Hidden/SSCC"
{
	Properties {
		_MainTex ("", any) = "" {}
	}

	CGINCLUDE

    #pragma target 3.0
    #pragma editor_sync_compilation

    #include "UnityCG.cginc"
        
    UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
    UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture);
    UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraGBufferTexture2);

    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

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

    CBUFFER_START(PerPassUpdatedUniforms)
    float4 _UVTransform;
    CBUFFER_END

    struct Attributes
    {
        float3 vertex : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    float2 TransformTriangleVertexToUV(float2 vertex)
    {
        float2 uv = (vertex + 1.0) * 0.5;
        return uv;
    }
    Varyings Vert_Default(Attributes input)
    {
        Varyings o;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_OUTPUT(Varyings, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.vertex = float4(input.vertex.xy, 0.0, 1.0);
        o.uv = TransformTriangleVertexToUV(input.vertex.xy);

        #if UNITY_UV_STARTS_AT_TOP
        o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif

        o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

        return o;
    }

    Varyings Vert_UVTransform(Attributes input)
    {
        Varyings o;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_OUTPUT(Varyings, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.vertex = float4(input.vertex.xy, 0.0, 1.0);
        o.uv = TransformTriangleVertexToUV(input.vertex.xy) * _UVTransform.xy + _UVTransform.zw;

        o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

        return o;
    }

	ENDCG

	SubShader 
	{
        LOD 100
		ZTest Always Cull Off ZWrite Off

        // 0
        Pass 
		{
            Name "Copy"

            CGPROGRAM
            #pragma vertex Vert_Default
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, input.uv);
            }
            ENDCG
        }

        // 1
        Pass 
		{
            Name "Composite"

            ColorMask RGB

            CGPROGRAM
			#pragma multi_compile_local __ DEBUG_EFFECT DEBUG_NORMALS
			#pragma multi_compile_local __ ORTHOGRAPHIC_PROJECTION
			#pragma multi_compile_local __ NORMALS_CAMERA NORMALS_RECONSTRUCT
			#pragma multi_compile_local CAVITY_SAMPLES_6 CAVITY_SAMPLES_8 CAVITY_SAMPLES_12
			#pragma multi_compile_local __ SATURATE_CAVITY
			#pragma multi_compile_local __ OUTPUT_TO_TEXTURE

			#pragma vertex Vert_UVTransform
			#pragma fragment Composite_Frag
				
			//
			inline half4 FetchSceneColor(float2 uv) {
				return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv);
			}
			inline float FetchRawDepth(float2 uv) {
				return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
			}
			inline float LinearizeDepth(float depth) {
				#if ORTHOGRAPHIC_PROJECTION
					#if UNITY_REVERSED_Z
						depth = 1 - depth;
					#endif
					float linearDepth = _ProjectionParams.y + depth * (_ProjectionParams.z - _ProjectionParams.y);
				#else
					float linearDepth = LinearEyeDepth(depth);
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
					/*float3 Pr, Pl, Pt, Pb;
					Pr = FetchViewPos(uv + float2(_Input_TexelSize.x, 0));
					Pl = FetchViewPos(uv + float2(-_Input_TexelSize.x, 0));
					Pt = FetchViewPos(uv + float2(0, _Input_TexelSize.y));
					Pb = FetchViewPos(uv + float2(0, -_Input_TexelSize.y));
					float3 N = normalize(cross(MinDiff(P, Pr, Pl), MinDiff(P, Pt, Pb)));*/
					//
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
					//
				#else
					#if NORMALS_CAMERA
						float3 N = DecodeViewNormalStereo(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture, uv));
					#else
						float3 N = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraGBufferTexture2, uv).rgb * 2.0 - 1.0;
						N = mul((float3x3)_WorldToCameraMatrix, N);
					#endif
					N = float3(N.x, -N.yz);
				#endif
			
				N = float3(N.x, -N.y, N.z);
				return N;
			}
			//
			#include "Shared.cginc"

            ENDCG
        }

	}

	FallBack off
}
