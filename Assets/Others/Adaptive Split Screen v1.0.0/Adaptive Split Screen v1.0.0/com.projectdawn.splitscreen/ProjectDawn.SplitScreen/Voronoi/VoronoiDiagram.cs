using System;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Unity.Collections;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// In mathematics, a Voronoi diagram is a partition of a plane into regions close to each of a given set of objects.
    /// In the simplest case, these objects are just finitely many points in the plane (called seeds, sites, or generators).
    /// https://en.wikipedia.org/wiki/Voronoi_diagram#:~:text=In%20mathematics%2C%20a%20Voronoi%20diagram,%2C%20sites%2C%20or%20generators).
    /// </summary>
    public struct VoronoiDiagram : IDisposable
    {
        public NativeList<float2> Vertices;
        public NativeList<VoronoiEdge> Edges;
        public NativeList<float2> Points;
        public NativeList<VoronoiRegion> Regions;
        public Rect Bounds;

        public bool IsCreated => Regions.IsCreated && Points.IsCreated;

        public VoronoiDiagram(Allocator allocator = Allocator.Temp)
        {
            Points = new NativeList<float2>(allocator);
            Regions = new NativeList<VoronoiRegion>(allocator);
            Vertices = new NativeList<float2>(allocator);
            Edges = new NativeList<VoronoiEdge>(allocator);
            Bounds = new Rect();
        }

        /// <summary>
        /// Returns points of the region.
        /// </summary>
        public NativeArray<float2> GetRegionPoints(in VoronoiRegion region)
        {
            CheckRegion(region);
            return Points.AsArray().GetSubArray(region.Offset, region.Length);
        }

        public void Clear()
        {
            Vertices.Clear();
            Edges.Clear();
            Points.Clear();
            Regions.Clear();
        }

         /// <summary>
        /// Returns area of the region.
        /// </summary>
        public float GetRegionArea(in VoronoiRegion region)
        {
            CheckRegion(region);
            if (region.Area != -1)
                return region.Area;
            var points = GetRegionPoints(region);

            var cacheRegion = Regions[region.Index];
            cacheRegion.Area = ConvexPolygon.GetArea(points);
            Regions[region.Index] = cacheRegion;

            return cacheRegion.Area;
        }

        public float GetDistanceBetweenRegions(in VoronoiRegion regionA, float scaleA, float2 offsetA, in VoronoiRegion regionB, float scaleB, float2 offsetB)
        {
            CheckRegion(regionA);
            CheckRegion(regionB);

            // Special case if sites very far from bounds
            if (regionA.Length == 0 || regionB.Length == 0)
                return float.MaxValue;

            var pointsA = GetRegionPoints(regionA);
            var pointsB = GetRegionPoints(regionB);
            return ConvexPolygon.GetDistance(pointsA, scaleA, offsetA, pointsB, scaleB, offsetB);
        }

        /// <summary>
        /// Returns centroid of the region.
        /// </summary>
        public float2 GetCentroid(in VoronoiRegion region)
        {
            CheckRegion(region);
            //return region.Centroid;
            var points = GetRegionPoints(region);
            return ConvexPolygon.GetCentroid(points);
        }

        /// <summary>
        /// Returns inscribed circle in region centroid.
        /// </summary>
        public float GetRegionInscribedCircleRadius(in VoronoiRegion region)
        {
            CheckRegion(region);
            var points = GetRegionPoints(region);
            return ConvexPolygon.GetInscribedCircleRadius(points, GetCentroid(region));
        }

        /// <summary>
        /// Returns maximum inscribed circle in region.
        /// </summary>
        public Circle GetRegionMaxInscribedCircle(in VoronoiRegion region, float relaxation = 1.6f)
        {
            CheckRegion(region);
            var points = GetRegionPoints(region);
            return ConvexPolygon.GetMaxInscribedCircle(points, relaxation);
        }

        /// <summary>
        /// Returns edge between two regions if exists.
        /// </summary>
        public bool TryFindEdgeBetweenRegions(in VoronoiRegion regionA, in VoronoiRegion regionB, out Line edge)
        {
            CheckRegion(regionA);
            CheckRegion(regionB);

            // Use edges if available
            if (Edges.Length != 0)
            {
                for (int edgeIndex = 0; edgeIndex < Edges.Length; ++edgeIndex)
                {
                    VoronoiEdge e = Edges[edgeIndex];
                    if ((e.LeftSiteIndex == regionA.Index && e.RightSiteIndex == regionB.Index) ||
                        (e.LeftSiteIndex == regionB.Index && e.RightSiteIndex == regionA.Index))
                        {
                        edge = e.Line;
                        return true;
                    }
                }
                edge = new Line();
                return false;
            }

            // If edges are not available try to find from points, it is less accurate solution
            var pointsA = GetRegionPoints(regionA);
            var pointsB = GetRegionPoints(regionB);

            float2 error = new float2(Bounds.size) * 0.01f;

            edge = new Line();

            for (int pointIndexA = 0; pointIndexA < pointsA.Length; ++pointIndexA)
            {
                var pointA = pointsA[pointIndexA];
                var nextPointA = pointsA[(pointIndexA + 1) % pointsA.Length];

                if (math.all(math.abs(pointA - nextPointA) <= error))
                    continue;

                for (int pointIndexB = 0; pointIndexB < pointsB.Length; ++pointIndexB)
                {
                    var pointB = pointsB[pointIndexB];
                    var nextPointB = pointsB[(pointIndexB + 1) % pointsB.Length];

                    if (math.all(math.abs(pointB - nextPointB) <= error))
                        continue;

                    float2 p0 = math.abs(pointA - nextPointB);
                    float2 p1 = math.abs(pointB - nextPointA);

                    if (math.all(p0 <= error) && math.all(p1 <= error))
                    {
                        edge = new Line(pointA, nextPointA);
                        return true;
                    }
                }
            }
            
            edge = new Line();
            return false;
        }

        public float CalculateAreaError()
        {
            float goalArea = (Bounds.width * Bounds.height) / Regions.Length;

            float error = 0;
            for (int i = 0; i <  Regions.Length; ++i)
            {
                var area = GetRegionArea(Regions[i]);
                float diff = goalArea - area;
                error += diff * diff;
            }
            return math.sqrt(error);
        }

        public void Dispose()
        {
            Points.Dispose();
            Regions.Dispose();
            Vertices.Dispose();
            Edges.Dispose();
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawRegion(in VoronoiRegion region, Color color)
        {
            CheckRegion(region);
            var points = Points.AsArray().Slice(region.Offset, region.Length);
            Gizmos.DrawConvexPolygon(points, 1, 0, color);
            Gizmos.DrawConvexPolygonWire(points, 1, 0, Color.white);
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawRegionPointOrder(in VoronoiRegion region, float size, Color color)
        {
#if UNITY_EDITOR
            CheckRegion(region);
            var points = Points.AsArray().Slice(region.Offset, region.Length);

            for (int i = 0; i < points.Length; ++i)
            {
                var point = points[i];
                var nextPoint = points[(i + 1) % points.Length];

                float2 midPoint = (nextPoint + point) / 2f;
                float2 direction = math.normalizesafe(nextPoint - point);
                quaternion rotation = quaternion.LookRotationSafe(new float3(direction.x, direction.y, 0), new float3(0, 0, 1));

                UnityEditor.Handles.color = color;
                UnityEditor.Handles.ArrowHandleCap(0, new Vector3(midPoint.x, midPoint.y, 0), rotation, size, EventType.Repaint);
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawRegion(in VoronoiRegion region, float scale, float2 offset, Color color)
        {
            CheckRegion(region);
            var points = Points.AsArray().Slice(region.Offset, region.Length);
            Gizmos.DrawConvexPolygon(points, scale, offset, color);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckRegion(in VoronoiRegion region)
        {
            if (!Regions.IsCreated)
                throw new Exception("Voronoi diagram regions must be created.");
            if (region.Index < 0 || region.Index >= Regions.Length)
                throw new ArgumentOutOfRangeException($"Region index {region.Index} is out of range in Regions.");
        }
    }
}
