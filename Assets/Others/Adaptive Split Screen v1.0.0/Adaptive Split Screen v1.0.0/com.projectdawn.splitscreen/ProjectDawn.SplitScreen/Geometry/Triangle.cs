using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    public struct Triangle
    {
        public float2 PointA;
        public float2 PointB;
        public float2 PointC;

        public Triangle(float2 a, float2 b, float2 c)
        {
            PointA = a;
            PointB = b;
            PointC = c;
        }

        public static bool GetCircumscribedCircleCenter(float2 a, float2 b, float2 c, out float2 point)
        {
            var l1 = new Line(a, b);
            var l2 = new Line(a, c);
            return Line.Intersection(l1, l2, out point);
        }

        /// <summary>
        /// Returns true if triangle points are in counter clockwise order.
        /// Based on https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        /// </summary>
        public static bool IsCounterClockwise(float2 a, float2 b, float2 c)
        {
            var m = new float3x3(
                new float3(a.x, a.y, 1),
                new float3(b.x, b.y, 1),
                new float3(c.x, c.y, 1));
            float determinant = math.determinant(m);
            return determinant > 0;
        }

        [Conditional("UNITY_EDITOR")]
        public void Draw(Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(new Vector3(PointA.x, PointA.y, 0), new Vector3(PointB.x, PointB.y, 0));
            UnityEditor.Handles.DrawLine(new Vector3(PointB.x, PointB.y, 0), new Vector3(PointC.x, PointC.y, 0));
            UnityEditor.Handles.DrawLine(new Vector3(PointC.x, PointC.y, 0), new Vector3(PointA.x, PointA.y, 0));
#endif
        }
    }
}
