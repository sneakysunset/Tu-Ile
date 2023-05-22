Shader "Hidden/ProjectDawn/Blit"
{
    Properties
    {
    }

    CGINCLUDE

        #include "UnityCG.cginc"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 texcoord : TEXCOORD0;
            float2 color : COLOR;
        };

        struct Varyings
        {
            float4 positionSS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 color : COLOR;
        };

        sampler2D _SplitScreenTexture;

        Varyings VertBlit(Attributes i)
        {
            Varyings o;
            o.positionSS = UnityObjectToClipPos(i.positionOS);
            o.uv = i.texcoord;
            o.color = i.color;
            return o;
        }

        half4 FragBlit(Varyings i) : SV_Target
        {
            half4 color = tex2D(_SplitScreenTexture, i.uv);

            // Out of bound sampling
            if (any(i.uv > 1 || i.uv < 0))
                return float4(0, 0, 0, 1);

            return color;
        }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "PreviewType" = "Plane"}

        Pass
        {
            CGPROGRAM

                #pragma vertex VertBlit
                #pragma fragment FragBlit

            ENDCG
        }
    }
}