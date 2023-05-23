Shader "Standard + Cavity"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_CurvatureMultiplier("Curvature Multiplier", Range(0,1.5)) = 1.0
		_CavityMultiplier("Cavity Multiplier", Range(0,1.5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Standard fullforwardshadows exclude_path:deferred 
		//Explaining "exclude_path:deferred":
		//we force shader to be in forward path, because we cant inject a custom texture in deferred, because it goes like this: *Start of the frame* ->
		//Generate GBuffer (we already need our custom texture here, but we'd need normals to generate it, but we dont have them yet, because they are not in the gbuffer, makes sense? It's a chicken and egg problem)
		//and thats why deferred with custom texture is reconstructed normals only, and also why you need your custom shaders to be lit in forward path
		//(deferred has an additional path where it runs all objects that dont have deferred compatible shaders through forward path)

		//Declare _SSCCTexture as input, then sample it in your shader by screenspace uv.
		sampler2D _SSCCTexture;

        float4 _Color;
		sampler2D _MainTex;
        float _Glossiness;
        float _Metallic;
		float _CurvatureMultiplier;
		float _CavityMultiplier;

        struct Input
        {
            float2 uv_MainTex;
			float4 screenPos;
        };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

			float2 screenUV = IN.screenPos.xy / IN.screenPos.w; //docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html
			float4 sscc = tex2D(_SSCCTexture, screenUV); //red is curvature [-1, +1], green is cavity [-1, +1], blue alpha are always 1
			if (sscc.b == 1.0) //This is how you can detect if 'screen' output mode is on and avoid reading the texture, this is just for housekeeping, as _SSCCTexture will be 0.5 grey in 'screen' mode
			{
				float curvature = sscc.r;
				float cavity = sscc.g;

				//You could totally for example split curvature or cavity by <0 and >0, isolating the dark/bright parts and, for example, coloring them separately.
				o.Albedo += (curvature * _CurvatureMultiplier) + (cavity * _CavityMultiplier);
			}

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
