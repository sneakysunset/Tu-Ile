using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public static class Blend
    {
        const float Half = 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendLine(
            float3 a, float3 b,
            float ab,
            out float3 p0, out float3 p1)
        {
            p0 = math.lerp(a, b, ab * Half);
            p1 = math.lerp(b, a, ab * Half);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendLine(
            float2 a, float2 b,
            float ab,
            out float2 p0, out float2 p1)
        {
            p0 = math.lerp(a, b, ab * Half);
            p1 = math.lerp(b, a, ab * Half);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendTriangle(
            float3 a, float3 b, float3 c,
            float ab, float bc, float ac,
            out float3 p0, out float3 p1, out float3 p2)
        {
            float d0 = 1f / (ab + ac + 1f);
            p0 = (a * 1f + b * ab + c * ac) * d0;

            float d1 = 1f / (ab + bc + 1f);
            p1 = (b * 1f + a * ab + c * bc) * d1;

            float d2 = 1f / (ac + bc + 1f);
            p2 = (c * 1f + a * ac + b * bc) * d2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendTriangle(
            float2 a, float2 b, float2 c,
            float ab, float bc, float ac,
            out float2 p0, out float2 p1, out float2 p2)
        {
            float d0 = 1f / (ab + ac + 1f);
            p0 = (a * 1f + b * ab + c * ac) * d0;

            float d1 = 1f / (ab + bc + 1f);
            p1 = (b * 1f + a * ab + c * bc) * d1;

            float d2 = 1f / (ac + bc + 1f);
            p2 = (c * 1f + a * ac + b * bc) * d2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendQuad(
            float3 a, float3 b, float3 c, float3 d, 
            float ab, float bc, float ac, float ad, float bd, float cd,
            out float3 p0, out float3 p1, out float3 p2, out float3 p3)
        {
            float d0 = 1f / (ab + ac + ad + 1f);
            p0 = (a * 1f + b * ab + c * ac + d * ad) * d0;

            float d1 = 1f / (ab + bc + bd + 1f);
            p1 = (b * 1f + a * ab + c * bc + d * bd) * d1;

            float d2 = 1f / (ac + bc + cd + 1f);
            p2 = (c * 1f + a * ac + b * bc + d * cd) * d2;

            float d3 = 1f / (ad + bd + cd + 1f);
            p3 = (d * 1f + a * ad + b * bd + c * cd) * d3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlendQuad(
            float2 a, float2 b, float2 c, float2 d, 
            float ab, float bc, float ac, float ad, float bd, float cd,
            out float2 p0, out float2 p1, out float2 p2, out float2 p3)
        {
            float divider0 = 1f / (ab + ac + ad + 1f);
            p0 = (a * 1f + b * ab + c * ac + d * ad) * divider0;

            float divider1 = 1f / (ab + bc + bd + 1f);
            p1 = (b * 1f + a * ab + c * bc + d * bd) * divider1;

            float divider2 = 1f / (ac + bc + cd + 1f);
            p2 = (c * 1f + a * ac + b * bc + d * cd) * divider2;

            float divider3 = 1f / (ad + bd + cd + 1f);
            p3 = (d * 1f + a * ad + b * bd + c * cd) * divider3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForceToQuad(
            ref float ab, ref float bc, ref float ac, ref float ad, ref float bd, ref float cd)
        {
            ForceToTriangle(ref ab, ref bd, ref ad);
            ForceToTriangle(ref ab, ref bc, ref ac);
            ForceToTriangle(ref bc, ref cd, ref bd);
            ForceToTriangle(ref ac, ref cd, ref ad);

            ForceToTriangle(ref ab, ref bd, ref ad);
            ForceToTriangle(ref ac, ref cd, ref ad);
            ForceToTriangle(ref ab, ref bc, ref ac);
            ForceToTriangle(ref bc, ref cd, ref bd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForceToTriangle(ref float a, ref float b, ref float c)
        {
            float _a = 1 - a;
            float _b = 1 - b;
            float _c = 1 - c;

            float ta = math.min(_a, _b + _c);
            float tb = math.min(_b, _c + _a);
            float tc = math.min(_c, _a + _b);

            a = 1 - ta;
            b = 1 - tb;
            c = 1 - tc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Linear(float distance, float blendStart, float blendEnd)
        {
            return math.saturate((distance - blendStart) / (blendEnd - blendStart));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LinearSafe(float distance, float blendStart, float blendEnd)
        {
            if ((blendStart - blendEnd) <= math.EPSILON)
            {
                return 0;
                //return (distance > blendStart) ? 0 : 1;
            }

            return Linear(distance, blendStart, blendEnd);
        }
    }
}
