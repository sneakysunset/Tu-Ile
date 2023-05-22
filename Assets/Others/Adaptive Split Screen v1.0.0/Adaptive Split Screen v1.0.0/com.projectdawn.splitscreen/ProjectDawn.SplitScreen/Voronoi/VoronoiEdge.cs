using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Voronoi edge of <see cref="VoronoiDiagram"/>.
    /// </summary>
    public struct VoronoiEdge
    {
        public int LeftSiteIndex;
        public int RightSiteIndex;
        public int FromVertexIndex;
        public int ToVertexIndex;
        public float2 Direction;
        public Line Line;
        public StandardLine StandardLine;
        public Range Range;

        public bool Valid => FromVertexIndex != -1;

        public static VoronoiEdge Null => new VoronoiEdge
        {
            FromVertexIndex = -1,
            ToVertexIndex = -1,
            LeftSiteIndex = -1,
            RightSiteIndex = -1,
        };

        [Conditional("UNITY_EDITOR")]
        public void Draw(NativeList<float2> vertices, Rect rect, Color color)
        {
            if (FromVertexIndex != -1 && ToVertexIndex != -1)
            {
                new Line(vertices[FromVertexIndex], vertices[ToVertexIndex]).Draw(color);
            }
            else if (FromVertexIndex != -1)
            {
                new Line(vertices[FromVertexIndex], vertices[FromVertexIndex] + Direction * 100000).Draw(color);
            }
            else if (ToVertexIndex != -1)
            {
                new Line(vertices[ToVertexIndex], vertices[ToVertexIndex] + Direction * 100000).Draw(color);
            }
        }
    }
}
