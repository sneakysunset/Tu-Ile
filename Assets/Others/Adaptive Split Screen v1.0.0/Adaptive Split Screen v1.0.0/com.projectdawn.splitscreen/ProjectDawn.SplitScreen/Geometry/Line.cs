using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    [System.Serializable]
    public struct Line
    {
        public float2 From;
        public float2 To;

        public float2 Direction => math.normalizesafe(To - From);
        public float2 MidPoint => (From + To) * 0.5f;
        public float Length => math.distance(From, To);
        public Line Reverse => new Line(To, From);

        public Line(float2 from, float2 to)
        {
            From = from;
            To = to;
        }

        public Line GetRight()
        {
            var direction = Direction;
            var right = new float2(direction.y, -direction.x);
            return new Line(MidPoint, MidPoint + right);
        }

        public Line GetLeft()
        {
            var direction = Direction;
            var left = new float2(-direction.y, direction.x);
            return new Line(MidPoint, MidPoint + left);
        }

        /// <summary>
        /// Returns minimum distance between line and point.
        /// </summary>
        public float GetMinimumDistanceFromPoint(float2 point)
        {
            return GetMinimumDistanceFromPoint(From, To, point);
        }

        // Based on https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        static float GetMinimumDistanceFromPoint(float2 v, float2 w, float2 p) 
        {
            // Return minimum distance between line segment vw and point p
            float l2 = math.lengthsq(w - v);  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0)
                return math.distance(p, v);   // v == w case
            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            float t = math.max(0, math.min(1, math.dot(p - v, w - v) / l2));
            float2 projection = v + t * (w - v);  // Projection falls on the segment
            return math.distance(p, projection);
        }

        /// <summary>
        /// Returns true if the point is on the right side of split volume by line.
        /// Split volume is refered here as the line extends to infinity this results dividing space into two mirror areas.
        /// </summary>
        public static bool IsPointRight(Line line, float2 point)
        {
            float2 towards = line.To - line.From;
            float length = math.length(towards);

            if (length < 1e-4f)
                return false;
            float2 direction = towards / length;

            var left = new float2(-direction.y, direction.x);
            var p = math.normalizesafe(point - line.From);
            float dot = left.x * p.x + left.y * p.y;
            return dot < 0;
        }

        /// <summary>
        /// Returns true if the point is on the left side of split volume by line.
        /// Split volume is refered here as the line extends to infinity this results dividing space into two mirror areas.
        /// </summary>
        public static bool IsPointLeft(Line line, float2 point)
        {
            float2 towards = line.To - line.From;
            float length = math.length(towards);

            if (length < 1e-4f)
                return false;
            float2 direction = towards / length;

            var left = new float2(direction.y, -direction.x);
            var p = math.normalizesafe(point - line.From);
            float dot = left.x * p.x + left.y * p.y;
            return dot < 0;
        }

        /// <summary>
        /// Returns intersection point between two lines.
        /// Based on https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        /// </summary>
        public static bool Intersection(Line a, Line b, out float2 point)
        {
            float x1 = a.From.x;
            float x2 = a.To.x;
            float x3 = b.From.x;
            float x4 = b.To.x;

            float y1 = a.From.y;
            float y2 = a.To.y;
            float y3 = b.From.y;
            float y4 = b.To.y;

            float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            // Both lines are parallel
            if (math.abs(denominator) <= math.EPSILON)
            {
                point = float2.zero;
                return false;
            }

            float s0 = (x1 * y2 - y1 * x2);
            float s1 = (x3 * y4 - y3 * x4);

            float px = s0 * (x3 - x4) - (x1 - x2) * s1;
            float py = s0 * (y3 - y4) - (y1 - y2) * s1;

            point = new float2(px / denominator, py / denominator);
            return true;
        }

        [Conditional("UNITY_EDITOR")]
        public void Draw(Color color)
        {
#if UNITY_EDITOR
            //UnityEditor.Handles.color = color;
            //UnityEditor.Handles.DrawLine(new Vector3(From.x, From.y, 0), new Vector3(To.x, To.y, 0));
            UnityEngine.Debug.DrawLine(new Vector3(From.x, From.y, 0), new Vector3(To.x, To.y, 0), color);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawArrowHead(Color color, float size)
        {
#if UNITY_EDITOR
            //UnityEditor.Handles.color = color;
            //UnityEditor.Handles.DrawLine(new Vector3(From.x, From.y, 0), new Vector3(To.x, To.y, 0));
            float2 direction = Direction;
            float angle = math.atan2(direction.y, direction.x);
            float leftAngle = angle + math.radians(-45);
            float rightAngle = angle + math.radians(45);
            float2 left = new float2(math.cos(leftAngle), math.sin(leftAngle)) * size;
            float2 right = new float2(math.cos(rightAngle), math.sin(rightAngle)) * size;
            UnityEngine.Debug.DrawLine(new Vector3(To.x - left.x, To.y - left.y, 0), new Vector3(To.x, To.y, 0), color);
            UnityEngine.Debug.DrawLine(new Vector3(To.x - right.x, To.y - right.y, 0), new Vector3(To.x, To.y, 0), color);
#endif
        }
    }
}
