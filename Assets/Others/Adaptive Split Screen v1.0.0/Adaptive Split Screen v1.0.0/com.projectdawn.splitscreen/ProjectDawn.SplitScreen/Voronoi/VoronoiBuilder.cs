using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Profiling;
using Unity.Jobs;
using Unity.Burst;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    internal enum VoronoiBuilderMode
    {
        DelaunayDual,

        BruteForce,
    }

    /// <summary>
    /// Voronoi builder for construcing <see cref="VoronoiDiagram"/> from sites.
    /// </summary>
    public struct VoronoiBuilder : IDisposable
    {
        const float OffsetOnFail = 1e-3f;
        readonly static float2 OffsetA = new float2(OffsetOnFail, 0);
        readonly static float2 OffsetB = new float2(-OffsetOnFail * 2, 0);
        readonly static float2 OffsetC = new float2(0, OffsetOnFail * 3);
        readonly static float2 OffsetD = new float2(0, -OffsetOnFail * 4);

        internal VoronoiBuilderMode Mode;
        internal NativeList<float2> Sites;
        public NativeList<float2> Vertices;
        public NativeList<VoronoiEdge> Edges;
        internal int NumSites;

        public bool IsCreated => Sites.IsCreated;

        public VoronoiBuilder(Allocator allocator = Allocator.Temp)
        {
            Sites = new NativeList<float2>(allocator);
            Vertices = new NativeList<float2>(allocator);
            Edges = new NativeList<VoronoiEdge>(allocator);
            Mode = VoronoiBuilderMode.DelaunayDual;
            NumSites = 0;
        }

        public void Clear()
        {
            Sites.Clear();
            Vertices.Clear();
            Edges.Clear();
        }

        public void Dispose()
        {
            Sites.Dispose();
            Vertices.Dispose();
            Edges.Dispose();
        }

        /// <summary>
        /// Builds voronoi diagram from sites.
        /// </summary>
        public void SetSites(float2 a, float2 b)
        {
            if (Mode == VoronoiBuilderMode.BruteForce)
            {
                Clear();
                Sites.Add(a);
                Sites.Add(b);
                return;
            }

            Clear();

            NumSites = 2;
            var l = new Line(a, b).GetLeft();
            Edges.Add(new VoronoiEdge
            {
                Direction = -l.Direction,
                StandardLine = StandardLine.PerpendicularLine(a, b),
                LeftSiteIndex = 1,
                RightSiteIndex = 0,
                FromVertexIndex = -1,
                ToVertexIndex = -1,
            });
        }

        /// <summary>
        /// Builds voronoi diagram from sites.
        /// </summary>
        public void SetSites(float2 a, float2 b, float2 c)
        {
            if (Mode == VoronoiBuilderMode.BruteForce)
            {
                Clear();
                Sites.Add(a);
                Sites.Add(b);
                Sites.Add(c);
                return;
            }

            if (!CreateVoronoiEdges(a, b, c))
            {
                // This can happen in two cases:
                // - All points form the line
                // In that case we move all the points by very little orthogonally
                bool success = CreateVoronoiEdges(
                    a + OffsetA, 
                    b + OffsetB, 
                    c + OffsetC);
                Assert.IsTrue(success);
            }
        }

        /// <summary>
        /// Builds voronoi diagram from sites using delaunay duals.
        /// As result algorithm will fail if delaunay dual triangles are not valid.
        /// </summary>
        public void SetSites(float2 s0, float2 s1, float2 s2, float2 s3)
        {
            if (Mode == VoronoiBuilderMode.BruteForce)
            {
                Clear();
                Sites.Add(s0);
                Sites.Add(s1);
                Sites.Add(s2);
                Sites.Add(s3);
                return;
            }

            if (!CreateVoronoiEdges(s0, s1, s2, s3))
            {
                // This can happen in two cases:
                // - All points form the line
                // - All points line on the circumcircle
                // - Two or more points are same
                // In that case we move all the points by very little orthogonally
                bool success = CreateVoronoiEdges(
                    s0 + OffsetA, 
                    s1 + OffsetB, 
                    s2 + OffsetC, 
                    s3 + OffsetD);
                Assert.IsTrue(success);
            }
        }

        /// <summary>
        /// Builds voronoi diagram from sites using delaunay duals.
        /// As result algorithm will fail if delaunay dual triangles are not valid.
        /// </summary>
        public void SetSites(NativeArray<float2> sites)
        {
            if (Mode == VoronoiBuilderMode.BruteForce)
            {
                Clear();
                Sites.AddRange(sites);
                return;
            }

            switch (sites.Length)
            {
                case 4:
                    SetSites(sites[0], sites[1], sites[2], sites[3]);
                    break;
                case 3:
                    SetSites(sites[0], sites[1], sites[2]);
                    break;
                case 2:
                    SetSites(sites[0], sites[1]);
                    break;
                default:
                    throw new NotImplementedException($"Voronoi builder does not work with {sites.Length}.");
            }
        }

        /// <summary>
        /// Build voronoi diagram that is clipped by bounds.
        /// </summary>
        public void Construct(ref VoronoiDiagram voronoiDiagram, Rect bounds)
        {
            voronoiDiagram.Clear();
            switch (Mode)
            {
                case VoronoiBuilderMode.DelaunayDual:
                    ClipEdgesWithBounds(Vertices, Edges, bounds);
                    CreateRegionsFromEdges(ref voronoiDiagram, Vertices, Edges, NumSites, bounds);
                    voronoiDiagram.Vertices.AddRange(Vertices);
                    voronoiDiagram.Edges.AddRange(Edges);
                    break;

                case VoronoiBuilderMode.BruteForce:
                    CreateVoronoiDiagram(ref voronoiDiagram, Sites, bounds);
                    break;
            }

        }

        static void ClipEdgesWithBounds(NativeList<float2> vertices, NativeList<VoronoiEdge> edges, Rect bounds)
        {
            for (int edgeIndex = 0; edgeIndex < edges.Length; ++edgeIndex)
            {
                VoronoiEdge edge = edges[edgeIndex];
                edge.Line = new Line();

                if (edge.FromVertexIndex != -1 && edge.ToVertexIndex != -1)
                {
                    var lineRange = new Range(0, 1);
                    if (GetLineRangeInRect(new Line(vertices[edge.FromVertexIndex], vertices[edge.ToVertexIndex]), edge.StandardLine, bounds, out Range boundRange, out Line clippedLine) &&
                        Combine(lineRange, boundRange, out Range boundedLineRange))
                    {
                        Line n;

                        if (boundRange.From < 0)
                        {
                            n.From = vertices[edge.FromVertexIndex];
                        }
                        else
                        {
                            n.From = clippedLine.From;
                        }

                        if (boundRange.To > 1)
                        {
                            n.To = vertices[edge.ToVertexIndex];
                        }
                        else
                        {
                            n.To = clippedLine.To;
                        }

                        edge.Line = n;
                        edge.Range = boundedLineRange;

                        edges[edgeIndex] = edge;
                    }
                    else
                    {
                        edges.RemoveAtSwapBack(edgeIndex);
                        edgeIndex--;
                    }
                }
                else if (edge.FromVertexIndex != -1)
                {
                    var lineRange = new Range(0, float.MaxValue);
                    if (GetLineRangeInRect(new Line(vertices[edge.FromVertexIndex], vertices[edge.FromVertexIndex] + edge.Direction), edge.StandardLine, bounds, out Range boundRange, out Line clippedLine) &&
                        Combine(lineRange, boundRange, out Range boundedLineRange))
                    {
                        Line n;

                        if (boundRange.From < 0)
                        {
                            n.From = vertices[edge.FromVertexIndex];
                        }
                        else
                        {
                            n.From = clippedLine.From;
                        }

                        n.To = clippedLine.To;

                        edge.Line = n;
                        edge.Range = boundedLineRange;

                        edges[edgeIndex] = edge;
                    }
                    else
                    {
                        edges.RemoveAtSwapBack(edgeIndex);
                        edgeIndex--;
                    }
                }
                else
                {
                    float2 center = bounds.center; // TODO
                    if (GetLineRangeInRect(new Line(center, center + edge.Direction), edge.StandardLine, bounds, out Range boundRange, out Line clippedLine))
                    {
                        Line n = clippedLine;

                        edge.Line = n;
                        edge.Range = boundRange;

                        edges[edgeIndex] = edge;
                    }
                    else
                    {
                        edges.RemoveAtSwapBack(edgeIndex);
                        edgeIndex--;
                    }
                }
            }
        }

        static void DrawPoint(float2 point, float size, Color color)
        {
            var origin = new Vector3(point.x, point.y);
            Debug.DrawLine(origin + new Vector3(-0.707f, -0.707f) * size, origin + new Vector3(0.707f, 0.707f) * size, color);
            Debug.DrawLine(origin + new Vector3(-0.707f, 0.707f) * size, origin + new Vector3(0.707f, -0.707f) * size, color);
        }

        static void CreateRegionsFromEdges(ref VoronoiDiagram voronoiDiagram, NativeList<float2> vertices, NativeList<VoronoiEdge> edges, int numSites, Rect bounds)
        {
            CheckVoronoiDiagram(voronoiDiagram);

            var Vertices = vertices;
            var Edges = edges;
            var Regions = voronoiDiagram.Regions;
            var Points = voronoiDiagram.Points;
            voronoiDiagram.Bounds = bounds;

            Regions.Clear();
            Points.Clear();

            float2 corner0 = new float2(bounds.xMin, bounds.yMin);
            float2 corner1 = new float2(bounds.xMax, bounds.yMin);
            float2 corner2 = new float2(bounds.xMax, bounds.yMax);
            float2 corner3 = new float2(bounds.xMin, bounds.yMax);

            for (int siteIndex = 0; siteIndex < numSites; ++siteIndex)
            {
                bool addCorner0 = true;
                bool addCorner1 = true;
                bool addCorner2 = true;
                bool addCorner3 = true;

                unsafe
                {
                    var isVertexAdded = stackalloc bool[Vertices.Length];
                    for (int i = 0; i < Vertices.Length; ++i)
                        isVertexAdded[i] = false;

                    int capacityPoints = numSites * numSites + 4;

                    var points = stackalloc float2[capacityPoints];

                    int numPoints = 0;

                    for (int edgeIndex = 0; edgeIndex < Edges.Length; ++edgeIndex)
                    {
                        VoronoiEdge edge = Edges[edgeIndex];

                        Assert.IsFalse(math.all(edge.Line.From == 0) && math.all(edge.Line.To == 0));

                        if (edge.LeftSiteIndex == siteIndex || edge.RightSiteIndex == siteIndex)
                        {
                            if (edge.Range.From != 0)
                            {
                                float2 point = edge.Line.From;
                                points[numPoints++] = point;
                            }
                            else if (!isVertexAdded[edge.FromVertexIndex])
                            {
                                float2 point = Vertices[edge.FromVertexIndex];
                                isVertexAdded[edge.FromVertexIndex] = true;
                                points[numPoints++] = point;
                            }

                            if (edge.Range.To != 1)
                            {
                                float2 point = edge.Line.To;
                                points[numPoints++] = point;
                            }
                            else if (!isVertexAdded[edge.ToVertexIndex])
                            {
                                float2 point = Vertices[edge.ToVertexIndex];
                                isVertexAdded[edge.ToVertexIndex] = true;
                                points[numPoints++] = point;
                            }
                        }

                        var line = edge.Line;
                        if (edge.LeftSiteIndex == siteIndex)
                        {
                            if (addCorner0 && Line.IsPointRight(line, corner0))
                                addCorner0 = false;
                            if (addCorner1 && Line.IsPointRight(line, corner1))
                                addCorner1 = false;
                            if (addCorner2 && Line.IsPointRight(line, corner2))
                                addCorner2 = false;
                            if (addCorner3 && Line.IsPointRight(line, corner3))
                                addCorner3 = false;
                        }
                        else if (edge.RightSiteIndex == siteIndex)
                        {
                            if (addCorner0 && Line.IsPointLeft(line, corner0))
                                addCorner0 = false;
                            if (addCorner1 && Line.IsPointLeft(line, corner1))
                                addCorner1 = false;
                            if (addCorner2 && Line.IsPointLeft(line, corner2))
                                addCorner2 = false;
                            if (addCorner3 && Line.IsPointLeft(line, corner3))
                            {
                                addCorner3 = false;
                                //line.Draw(Color.red);
                            }
                        }
                    }

                    int regionIndex = siteIndex;

                    // Usually it happens if the region is outside the bounds
                    if (numPoints == 0)
                    {
                        Regions.Add(new VoronoiRegion
                        {
                            Index = regionIndex,
                            Offset = 0,
                            Length = 0,
                            Centroid = float2.zero,
                            Area = -1,
                        });
                        continue;
                    }

                    // Add bounds corner points
                    if (addCorner0)
                    {
                        points[numPoints++] = corner0;
                    }
                    if (addCorner1)
                    {
                        points[numPoints++] = corner1;
                    }
                    if (addCorner2)
                    {
                        points[numPoints++] = corner2;
                    }
                    if (addCorner3)
                    {
                        points[numPoints++] = corner3;
                    }

                    int regionPointsStartIndex = Points.Length;
                    Points.AddRange(points, numPoints);

                    var regionPoints = Points.AsArray().GetSubArray(regionPointsStartIndex, numPoints);

                    ConvexPolygon.SortCounterClockwise(regionPoints);

                    var centroid = ConvexPolygon.GetCentroid(regionPoints);

                    var region = new VoronoiRegion
                    {
                        Index = regionIndex,
                        Offset = regionPointsStartIndex,
                        Length = numPoints,
                        Centroid = centroid,
                        Area = -1,
                    };

                    // Add polygon into voroinoi diagram
                    Regions.Add(region);
                }
            }
        }

        static bool Combine(Range lhs, Range rhs, out Range combined)
        {
            float min = math.max(lhs.From, rhs.From);
            float max = math.min(lhs.To, rhs.To);
            if (max < min)
            {
                combined = new Range();
                return false;
            }

            combined = new Range(min, max);
            return true;
        }

        static bool GetLineRangeInRect(Line l, StandardLine line, Rect bounds, out Range range, out Line clippedLine)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            float2 minPoint = float2.zero;
            float2 maxPoint = float2.zero;

            //line = new StandardLine(l.From, l.To);

            float2 left = new float2(bounds.xMin, line.SolveY(bounds.xMin));
            if (bounds.yMin <= left.y && left.y <= bounds.yMax)
            {
                //DrawPoint(left, 0.1f, Color.white);
                float t = (left.x - l.From.x) / (l.To.x - l.From.x);
                if (t < min)
                {
                    min = t;
                    minPoint = left;
                }
                if (t > max)
                {
                    max = t;
                    maxPoint = left;
                }
            }

            float2 right = new float2(bounds.xMax, line.SolveY(bounds.xMax));
            if (bounds.yMin <= right.y && right.y <= bounds.yMax)
            {
                //DrawPoint(right, 0.1f, Color.white);
                float t = (right.x - l.From.x) / (l.To.x - l.From.x);
                if (t < min)
                {
                    min = t;
                    minPoint = right;
                }
                if (t > max)
                {
                    max = t;
                    maxPoint = right;
                }
            }

            float2 bottom = new float2(line.SolveX(bounds.yMin), bounds.yMin);
            if (bounds.xMin <= bottom.x && bottom.x <= bounds.xMax)
            {
                //DrawPoint(bottom, 0.1f, Color.white);
                float t = (bottom.y - l.From.y) / (l.To.y - l.From.y);
                if (t < min)
                {
                    min = t;
                    minPoint = bottom;
                }
                if (t > max)
                {
                    max = t;
                    maxPoint = bottom;
                }
            }

            float2 up = new float2(line.SolveX(bounds.yMax), bounds.yMax);
            if (bounds.xMin <= up.x && up.x <= bounds.xMax)
            {
                //DrawPoint(up, 0.1f, Color.white);
                float t = (up.y - l.From.y) / (l.To.y - l.From.y);
                if (t < min)
                {
                    min = t;
                    minPoint = up;
                }
                if (t > max)
                {
                    max = t;
                    maxPoint = up;
                }
            }

            clippedLine = new Line(minPoint, maxPoint);

            if (max < min)
            {
                range = new Range();
                return false;
            }

            range = new Range(min, max);
            return true;
        }

        static bool IsDelaunay(float2 a, float2 b, float2 c, float2 d, int s0, int s1, int s2, int index, ref VoronoiEdge e0, ref VoronoiEdge e1, ref VoronoiEdge e2, out float2 point)
        {
            bool reverse = false;
            if (!Triangle.IsCounterClockwise(a, b, c))
            {
                // Reverse triangle order
                float2 temp = a;
                a = c;
                c = temp;
                reverse = true;
            }

            var m = new float4x4(
                new float4(a.x, a.y, a.x*a.x + a.y*a.y, 1),
                new float4(b.x, b.y, b.x*b.x + b.y*b.y, 1),
                new float4(c.x, c.y, c.x*c.x + c.y*c.y, 1),
                new float4(d.x, d.y, d.x*d.x + d.y*d.y, 1)
            );

            float determinant = math.determinant(m);
            if (determinant < 0)
            {
                var l0 = new Line(a, b).GetLeft();
                var l1 = new Line(b, c).GetLeft();
                var l2 = new Line(c, a).GetLeft();

                bool s = Line.Intersection(l0, l1, out point);
                if (!s)
                    return false;

                if (reverse)
                {
                    // cb
                    // ba
                    // ac
                    e0.Direction = -l1.Direction;
                    e1.Direction = -l0.Direction;
                    e2.Direction = -l2.Direction;

                    e0.StandardLine = StandardLine.PerpendicularLine(b, c);
                    e1.StandardLine = StandardLine.PerpendicularLine(a, b);
                    e2.StandardLine = StandardLine.PerpendicularLine(c, a);

                    e1.RightSiteIndex = s2;
                    e1.LeftSiteIndex = s1;

                    e0.RightSiteIndex = s1;
                    e0.LeftSiteIndex = s0;

                    e2.RightSiteIndex = s0;
                    e2.LeftSiteIndex = s2;
                }
                else
                {
                    // ab
                    // bc
                    // ca
                    e0.Direction = -l0.Direction;
                    e1.Direction = -l1.Direction;
                    e2.Direction = -l2.Direction;

                    e0.StandardLine = StandardLine.PerpendicularLine(a, b);
                    e1.StandardLine = StandardLine.PerpendicularLine(b, c);
                    e2.StandardLine = StandardLine.PerpendicularLine(c, a);

                    e0.RightSiteIndex = s0;
                    e0.LeftSiteIndex = s1;

                    e1.RightSiteIndex = s1;
                    e1.LeftSiteIndex = s2;

                    e2.RightSiteIndex = s2;
                    e2.LeftSiteIndex = s0;
                }


                if (e0.FromVertexIndex == -1)
                    e0.FromVertexIndex = index;
                else
                {
                    e0.ToVertexIndex = e0.FromVertexIndex;
                    e0.FromVertexIndex = index;
                }

                if (e1.FromVertexIndex == -1)
                    e1.FromVertexIndex = index;
                else
                {
                    e1.ToVertexIndex = e1.FromVertexIndex;
                    e1.FromVertexIndex = index;
                }

                if (e2.FromVertexIndex == -1)
                    e2.FromVertexIndex = index;
                else
                {
                    e2.ToVertexIndex = e2.FromVertexIndex;
                    e2.FromVertexIndex = index;
                }

                Assert.IsTrue(s);
                return true;
            }
            point = 0;
            return false;
        }

        bool CreateVoronoiEdges(float2 a, float2 b, float2 c)
        {
            bool reverse = false;
            if (!Triangle.IsCounterClockwise(a, b, c))
            {
                // Reverse triangle order
                float2 temp = a;
                a = c;
                c = temp;
                reverse = true;
            }

            var l0 = new Line(a, b).GetLeft();
            var l1 = new Line(b, c).GetLeft();
            var l2 = new Line(c, a).GetLeft();

            if (!Line.Intersection(l0, l1, out float2 point))
                return false;

            Clear();
            NumSites = 3;

            var e0 = VoronoiEdge.Null;
            var e1 = VoronoiEdge.Null;
            var e2 = VoronoiEdge.Null;

            int s0 = 0;
            int s1 = 1;
            int s2 = 2;

            Vertices.Add(point);

            if (reverse)
            {
                // cb
                // ba
                // ac
                e0.Direction = -l1.Direction;
                e1.Direction = -l0.Direction;
                e2.Direction = -l2.Direction;

                e0.StandardLine = StandardLine.PerpendicularLine(b, c);
                e1.StandardLine = StandardLine.PerpendicularLine(a, b);
                e2.StandardLine = StandardLine.PerpendicularLine(c, a);

                e1.RightSiteIndex = s2;
                e1.LeftSiteIndex = s1;

                e0.RightSiteIndex = s1;
                e0.LeftSiteIndex = s0;

                e2.RightSiteIndex = s0;
                e2.LeftSiteIndex = s2;
            }
            else
            {
                // ab
                // bc
                // ca
                e0.Direction = -l0.Direction;
                e1.Direction = -l1.Direction;
                e2.Direction = -l2.Direction;

                e0.StandardLine = StandardLine.PerpendicularLine(a, b);
                e1.StandardLine = StandardLine.PerpendicularLine(b, c);
                e2.StandardLine = StandardLine.PerpendicularLine(c, a);

                e0.RightSiteIndex = s0;
                e0.LeftSiteIndex = s1;

                e1.RightSiteIndex = s1;
                e1.LeftSiteIndex = s2;

                e2.RightSiteIndex = s2;
                e2.LeftSiteIndex = s0;
            }

            e0.FromVertexIndex = 0;
            e1.FromVertexIndex = 0;
            e2.FromVertexIndex = 0;

            Edges.Add(e0);
            Edges.Add(e1);
            Edges.Add(e2);

            return true;
        }

        bool CreateVoronoiEdges(float2 s0, float2 s1, float2 s2, float2 s3)
        {
            var vertices = Vertices;
            var edges = Edges;

            var e01 = VoronoiEdge.Null;
            var e02 = VoronoiEdge.Null;
            var e03 = VoronoiEdge.Null;
            var e12 = VoronoiEdge.Null;
            var e13 = VoronoiEdge.Null;
            var e23 = VoronoiEdge.Null;

            int numTriangles = 0;

            //int4 t0 = new int4(0, 1, 2, 3);
            bool t012 = IsDelaunay(s0, s1, s2, s3, 0, 1, 2, numTriangles, ref e01, ref e12, ref e02, out float2 v012);
            if (t012)
            {
                numTriangles++;
            }

            //int4 t1 = new int4(0, 1, 3, 2);
            bool t013 = IsDelaunay(s0, s1, s3, s2, 0, 1, 3, numTriangles, ref e01, ref e13, ref e03, out float2 v013);
            if (t013)
            {
                numTriangles++;
            }

            //int4 t2 = new int4(0, 2, 3, 1);
            bool t023 = IsDelaunay(s0, s2, s3, s1, 0, 2, 3, numTriangles, ref e02, ref e23, ref e03, out float2 v023);
            if (t023)
            {
                numTriangles++;
            }

            //int4 t3 = new int4(1, 2, 3, 0);
            bool t123 = IsDelaunay(s1, s2, s3, s0, 1, 2, 3, numTriangles, ref e12, ref e23, ref e13, out float2 v123);
            if (t123)
            {
                numTriangles++;
            }

            if (numTriangles != 3 && numTriangles != 2)
                return false;

            Clear();
            NumSites = 4;

            int i012 = -1;
            if (t012)
            {
                i012 = vertices.Length;
                vertices.Add(v012);
            }

            int i013 = -1;
            if (t013)
            {
                i013 = vertices.Length;
                vertices.Add(v013);
            }

            int i023 = -1;
            if (t023)
            {
                i023 = vertices.Length;
                vertices.Add(v023);
            }

            int i123 = -1;
            if (t123)
            {
                i123 = vertices.Length;
                vertices.Add(v123);
            }

            if (e01.Valid)
                edges.Add(e01);
            if (e02.Valid)
                edges.Add(e02);
            if (e03.Valid)
                edges.Add(e03);
            if (e12.Valid)
                edges.Add(e12);
            if (e13.Valid)
                edges.Add(e13);
            if (e23.Valid)
                edges.Add(e23);

            return true;
        }

        /// <summary>
        /// Builds voronoi diagram from sites inside bounds.
        /// Based on http://www.mathforgameprogrammers.com/gdc2016/GDC2016_Eiserloh_Squirrel_JuicingYourCameras.pdf.
        /// </summary>
        static void CreateVoronoiDiagram(ref VoronoiDiagram voronoiDiagram, NativeArray<float2> sites, Rect bounds)
        {
            CheckVoronoiDiagram(voronoiDiagram);

            voronoiDiagram.Bounds = bounds;

            var Regions = voronoiDiagram.Regions;
            var Points = voronoiDiagram.Points;
            var Bounds = bounds;

            // Clear previous
            Regions.Clear();
            Points.Clear();

            var numRegions = sites.Length;

            // Create lines from bounds
            // d ---2--- c
            // |         |
            // 3         1
            // |         |
            // a ---0--- b
            var boundsPointA = new float2(bounds.xMin, bounds.yMin);
            var boundsPointB = new float2(bounds.xMax, bounds.yMin);
            var boundsPointC = new float2(bounds.xMax, bounds.yMax);
            var boundsPointD = new float2(bounds.xMin, bounds.yMax);
            var boundsLine0 = new Line(boundsPointA, boundsPointB);
            var boundsLine1 = new Line(boundsPointB, boundsPointC);
            var boundsLine2 = new Line(boundsPointC, boundsPointD);
            var boundsLine3 = new Line(boundsPointD, boundsPointA);

            unsafe
            {
                int capacityEdges = numRegions - 1;
                int capacityPoints = numRegions * numRegions + 4;

                var points = stackalloc float2[capacityPoints];
                var edges = stackalloc Line[capacityEdges];

                for (int regionIndex = 0; regionIndex < numRegions; ++regionIndex)
                {
                    // Find lines between sites
                    int numEdges = 0;
                    for (int regionIndexB = 0; regionIndexB < numRegions; ++regionIndexB)
                    {
                        if (regionIndex == regionIndexB)
                            continue;

                        float2 pointA = sites[regionIndex];
                        float2 pointB = sites[regionIndexB];

                        var edge = new Line(pointA, pointB);

                        float2 direction = edge.Direction;
                        float2 midPoint = edge.MidPoint;
                        var left = new float2(-direction.y, direction.x);
                        var line = new Line(midPoint - left * 10, midPoint + left * 10);

                        edges[numEdges] = line;

                        numEdges++;
                    }

                    Assert.IsTrue(numEdges <= capacityEdges);

                    int numPoints = 0;

                    float2 error = 0.00001f * new float2(Bounds.size);

                    // Got through all edge pairs and find intersection points
                    for (int edgeIndexA = 0; edgeIndexA < numEdges; ++edgeIndexA)
                    {
                        Line edgeA = edges[edgeIndexA];

                        for (int edgeIndexB = edgeIndexA + 1; edgeIndexB < numEdges; ++edgeIndexB)
                        {
                            Line edgeB = edges[edgeIndexB];

                            bool intersection = Line.Intersection(edgeA, edgeB, out var point);

                            if (intersection && BoundsContainPoint(bounds, point, error))
                            {
                                points[numPoints++] = point;
                            }
                        }

                        // Also find intersection with bound lines
                        if (Line.Intersection(edgeA, boundsLine0, out float2 point0) && BoundsContainPoint(bounds, point0, error))
                            points[numPoints++] = point0;
                        if (Line.Intersection(edgeA, boundsLine1, out float2 point1) && BoundsContainPoint(bounds, point1, error))
                            points[numPoints++] = point1;
                        if (Line.Intersection(edgeA, boundsLine2, out float2 point2) && BoundsContainPoint(bounds, point2, error))
                            points[numPoints++] = point2;
                        if (Line.Intersection(edgeA, boundsLine3, out float2 point3) && BoundsContainPoint(bounds, point3, error))
                            points[numPoints++] = point3;
                    }

                    // Add bound points
                    points[numPoints++] = boundsPointA;
                    points[numPoints++] = boundsPointB;
                    points[numPoints++] = boundsPointC;
                    points[numPoints++] = boundsPointD;

                    Assert.IsTrue(numPoints <= capacityPoints);

                    // Remove all points that are not within the region polygon
                    int regionPointsStartIndex = Points.Length;
                    for (int pointIndex = 0; pointIndex < numPoints; ++pointIndex)
                    {
                        var point = points[pointIndex];

                        bool addPoint = true;

                        for (int normalIndex = 0; normalIndex < numEdges; ++normalIndex)
                        {
                            var edge = edges[normalIndex];

                            float2 normalDirection = edge.Direction;
                            float2 pointToNormalDirection = math.normalizesafe(point - edge.From);

                            float crossProduct = normalDirection.x * pointToNormalDirection.y - normalDirection.y * pointToNormalDirection.x;
                            if (crossProduct < -0.0001f * Bounds.size.x)
                            {
                                addPoint = false;
                                break;
                            }
                        }

                        if (addPoint)
                        {
                            Points.Add(point);
                        }
                    }
                    
                    int regionPointsEndIndex = Points.Length;
                    int numRegionPoints = regionPointsEndIndex - regionPointsStartIndex;

                    var regionPoints = Points.AsArray().GetSubArray(regionPointsStartIndex, numRegionPoints);

                    // Order polygon points to be counter clockwise
                    ConvexPolygon.SortCounterClockwise(regionPoints);

                    var region = new VoronoiRegion
                    {
                        Index = regionIndex,
                        Offset = regionPointsStartIndex,
                        Length = numRegionPoints,
                        Centroid = ConvexPolygon.GetCentroid(regionPoints),
                        Area = -1,
                    };

                    // Add polygon into voroinoi diagram
                    Regions.Add(region);
                }
            }
        }

        static bool BoundsContainPoint(Rect bounds, float2 point, float2 error)
        {
            return math.all((float2)bounds.min - error <= point & point <= (float2)bounds.max + error);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckVoronoiDiagram(in VoronoiDiagram voronoiDiagram)
        {
            if (!voronoiDiagram.IsCreated)
                throw new Exception("Voronoi diagram must be created.");
        }
    }
}
