using System.Diagnostics;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class Gizmos
    {
        [Conditional("UNITY_EDITOR")]
        public static void DrawConvexPolygon(NativeSlice<float2> points, float scale, float2 offset, Color color)
        {
#if UNITY_EDITOR
            Vector3[] vertices = new Vector3[points.Length];
            for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
            {
                vertices[pointIndex] = new Vector3(points[pointIndex].x * scale + offset.x, points[pointIndex].y  * scale + offset.y);
            }
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawAAConvexPolygon(vertices);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawConvexPolygonWire(NativeSlice<float2> points, float scale, float2 offset, Color color)
        {
#if UNITY_EDITOR
            Vector3[] vertices = new Vector3[points.Length + 1];
            for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
            {
                vertices[pointIndex] = new Vector3(points[pointIndex].x * scale + offset.x, points[pointIndex].y  * scale + offset.y);
            }
            vertices[points.Length] = vertices[0];

            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawAAPolyLine(vertices);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawPoint(float2 point, float size, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color * 0.8f;
            UnityEditor.Handles.DrawSolidArc(new Vector3(point.x, point.y, 0), -Vector3.forward, Vector3.up, 360, size);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawPointWire(Circle circle, Color color)
        {
            DrawPointWire(circle.Center, circle.Radius, color);
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawPointWire(float2 point, float size, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color * 0.8f;
            UnityEditor.Handles.DrawWireArc(new Vector3(point.x, point.y, 0), -Vector3.forward, Vector3.up, 360, size);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawBounds(Rect rect, float size, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMin, 0), new Vector3(rect.xMax, rect.yMin, 0), size);
            UnityEditor.Handles.DrawDottedLine(new Vector3(rect.xMax, rect.yMin, 0), new Vector3(rect.xMax, rect.yMax, 0), size);
            UnityEditor.Handles.DrawDottedLine(new Vector3(rect.xMax, rect.yMax, 0), new Vector3(rect.xMin, rect.yMax, 0), size);
            UnityEditor.Handles.DrawDottedLine(new Vector3(rect.xMin, rect.yMax, 0), new Vector3(rect.xMin, rect.yMin, 0), size);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawBounds(Bounds bounds, Color color)
        {
#if UNITY_EDITOR
            UnityEngine.Gizmos.color = color;
            UnityEngine.Gizmos.DrawCube(bounds.center, bounds.size);
            UnityEngine.Gizmos.color = new Color(color.r, color.g, color.b, 1);
            UnityEngine.Gizmos.DrawWireCube(bounds.center, bounds.size);
#endif
        }
    }
}
