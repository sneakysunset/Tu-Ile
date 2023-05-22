using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class DelaunayDual
    {
        /// <summary>
        /// Finds delauney dual triangles from given 4 points.
        /// </summary>
        public static bool GetTriangles(float2 a, float2 b, float2 c, float2 d, ref NativeList<Triangle> triangles)
        {
            triangles.Clear();

            if (IsDelaunay(a, b, c, d))
            {
                triangles.Add(new Triangle(a, b, c));
            }

            if (IsDelaunay(a, b, d, c))
            {
                triangles.Add(new Triangle(a, b, d));
            }

            if (IsDelaunay(a, c, d, b))
            {
                triangles.Add(new Triangle(a, c, d));
            }

            if (IsDelaunay(b, c, d, a))
            {
                triangles.Add(new Triangle(b, c, d));
            }

            return triangles.Length != 0;
        }

        // Based on https://en.wikipedia.org/wiki/Delaunay_triangulation
        static bool IsDelaunay(float2 a, float2 b, float2 c, float2 d)
        {
            if (!Triangle.IsCounterClockwise(a, b, c))
            {
                // Reverse triangle order
                float2 temp = a;
                a = c;
                c = temp;
            }

            var m = new float4x4(
                new float4(a.x, a.y, a.x*a.x + a.y*a.y, 1),
                new float4(b.x, b.y, b.x*b.x + b.y*b.y, 1),
                new float4(c.x, c.y, c.x*c.x + c.y*c.y, 1),
                new float4(d.x, d.y, d.x*d.x + d.y*d.y, 1)
            );

            float determinant = math.determinant(m);
            return determinant <= 0;
        }
    }
}
