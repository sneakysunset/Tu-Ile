Shader "Hidden/ProjectDawn/Split"
{
    Properties
    {
        _Texture("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "PreviewType" = "Plane"}

        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionSS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _Texture;
            float4 _Color;

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionSS = UnityObjectToClipPos(i.positionOS);
                o.uv = i.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                return _Color * tex2D(_Texture, i.uv);
            }

            ENDCG
        }
    }
}