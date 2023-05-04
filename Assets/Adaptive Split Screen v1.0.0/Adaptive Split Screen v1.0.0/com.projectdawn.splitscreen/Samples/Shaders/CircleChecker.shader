Shader "Unlit/CircleChecker"
{
    Properties
    {
        _ColorA("Color A", Color) = (1, 1, 1, 1)
        _ColorB("Color B", Color) = (0, 0, 0, 1)
        _OuterColumns("Outer Columns", Int) = 16
        _InnerColumns("Inner Columns", Int) = 2
        _RowScale("Row Scale", Float) = 200.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            #define PI 3.1415926535897932384626433832795

            half4 _ColorA;
            half4 _ColorB;
            float _OuterColumns;
            float _InnerColumns;
            float _RowScale;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionSS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // GLSL mod
            float mod(float a, float b)
            {
                return fmod(abs(a), b);
            }

            // Taken renderBackgroundRadialChecker from https://www.shadertoy.com/view/4sVXR1
            half4 BackgroundRadialChecker(half4 colorA, half4 colorB, int outerColumns, int innerColumns, float rowScale, float2 uv)
            {
                float2 worldCoord = uv * 2 - 1;

                float radius = length(worldCoord.xy);
                float angularD = atan2(worldCoord.y, worldCoord.x) * radius;

                const float rowWidthGrowthPow = 2.5;
                const float rowRadiusScale = rowScale;
                // index of a radial checker row.
                float row = ceil(pow(radius * rowRadiusScale, 1.0 / rowWidthGrowthPow));
                
                float rowOuterRadius = pow(row, rowWidthGrowthPow) / rowRadiusScale;    
                float radialBorderD = abs(radius - rowOuterRadius);
                float columns = 10.0;
                const float minRow = 2.0;
                if (row < minRow + 0.1)
                {
                    row = minRow;
                    columns = innerColumns;
                }
                else
                {
                    float rowInnerRadius = pow(row - 1.0, rowWidthGrowthPow) / rowRadiusScale;
                    radialBorderD = min(radialBorderD, abs(radius - rowInnerRadius));

                    columns = outerColumns;
                }

                // index of a concentric checker column.
                float columnWidth = (radius * 2.0 * PI) / columns;
                float col = ceil(angularD / columnWidth);

                float concentricBorderD = abs(mod(angularD / columnWidth + 0.5, 1.0) - 0.5) * columnWidth;

                float3 white = 1;
                float3 black = 0;
                float3 midColor = 0.5 * (black + white);

                half4 color;
                if (mod(row + col, 2.0) > 0.5)
                {
                    color = colorA;
                }
                else
                {
                    color = colorB;
                }

                //float borderDPx = camWorld2Px(cam, min(radialBorderD, concentricBorderD));
                //float aaColorWeight = smoothstep(0.0, 0.6, borderDPx);

                return color;//lerp(midColor, color, 0/*aaColorWeight*/);
            }

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionSS = UnityObjectToClipPos(i.positionOS);
                o.uv = i.texcoord;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                return BackgroundRadialChecker(_ColorA, _ColorB, _OuterColumns, _InnerColumns, _RowScale, i.uv);
            }
            ENDCG
        }
    }
}
