using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    [System.Serializable]
    /// <summary>
    /// Line in standard form ax + by = c.
    /// </summary>
    public struct StandardLine
    {
        public float a;
        public float b;
        public float c;

        public StandardLine(float a, float b, float c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public StandardLine(float2 p0, float2 p1)
        {
            float x1 = p0.x;
            float y1 = p0.y;
            float x2 = p1.x;
            float y2 = p1.y;

            a = y1 - y2;
            b = x2 - x1;
            c = -((x1 - x2) * y1 + (y2 - y1) * x1);
        }

        /// <summary>
        /// Returns x value at y axis.
        /// </summary>
        public float SolveX(float y)
        {
            return (c - (b * y)) / a;
        }

        /// <summary>
        /// Returns y value at x axis.
        /// </summary>
        public float SolveY(float x)
        {
            return (c - (a * x)) / b;
        }

        /// <summary>
        /// Returns perpendicular line to specified points.
        /// </summary>
        public static StandardLine PerpendicularLine(float2 s0, float2 s1)
        {
            float dx = s1.x - s0.x;
            float dy = s1.y - s0.y;
            float c = s0.x * dx + s0.y * dy + (dx*dx + dy*dy) * 0.5f;

            return new StandardLine(dx, dy, c);
        }

        /// <summary>
        /// Returns intersection point between two lines.
        /// Based on https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        /// </summary>
        public static bool Intersection(StandardLine a, StandardLine b, out float2 point)
        {
            float a1 = a.a;
            float b1 = a.b;
            float c1 = -a.c;

            float a2 = b.a;
            float b2 = b.b;
            float c2 = -b.c;

            float cp = (a1 * b2) - (a2 * b1);
            if (cp == 0)
            {
                point = float2.zero;
                return false;
            }

            float ap = (b1 * c2) - (b2 * c1);
            float bp = (a2 * c1) - (a1 * c2);

            point = new float2(ap / cp, bp / cp);
            return true;
        }

        [Conditional("UNITY_EDITOR")]
        public void Draw(Rect rect, Color color)
        {
#if UNITY_EDITOR
            float2 p0;
            float2 p1;
            if (a > b)
            {
                p0 = new float2(rect.xMin, SolveY(rect.xMin));
                p1 = new float2(rect.xMax, SolveY(rect.xMax));
            }
            else
            {
                p0 = new float2(SolveX(rect.yMin), rect.yMin);
                p1 = new float2(SolveX(rect.yMax), rect.yMax);
            }
            UnityEngine.Debug.DrawLine(new Vector3(p0.x, p0.y, 0), new Vector3(p1.x, p1.y, 0), color);
#endif
        }
    }
}
