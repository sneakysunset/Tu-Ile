using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class ConvexPolygon
    {
        /// <summary>
        /// Returns maximum inscribed circle in convex polygon.
        /// Polygon points must be convex and sorted counter clockwise.
        /// </summary>
        public static Circle GetMaxInscribedCircle(NativeArray<float2> points, float relaxation = 1.6f)
        {
            return MaxInscribedCircle.GetConvexPolygonMaxInscribedCircle(points, GetCentroid(points), relaxation);
        }

        /// <summary>
        /// Returns incribed circle with given center.
        /// Polygon points must be convex and sorted counter clockwise.
        /// </summary>
        public static float GetInscribedCircleRadius(NativeArray<float2> points, float2 center)
        {
            return MaxInscribedCircle.GetInscribedCircleRadius(points, center);
        }

        /// <summary>
        /// Triangules polygon with its center as triangles strip.
        /// Polygon points must be convex and sorted counter clockwise.
        /// </summary>
        public static void Triangulate(NativeSlice<float2> points, float2 centroid, float2 uvCenter, ref NativeList<float3> vertices, ref NativeList<float2> uvs, ref NativeList<int> indices)
        {
            // Triangulate wiht triangle strip

            // Add center
            {
                var vertex = centroid;

                vertices.Add(new float3(vertex.x, vertex.y, 0));

                var uv = (new float2(vertex.x, vertex.y) - uvCenter) * 0.5f + 0.5f;

                uvs.Add(uv);
            }

            for (int i = 0; i < points.Length; ++i)
            {
                var vertex = points[i];

                vertices.Add(new float3(vertex.x, vertex.y, 0));

                var uv = (new float2(vertex.x, vertex.y) - uvCenter) * 0.5f + 0.5f;

                uvs.Add(uv);
            }

            for (int i = 1; i < vertices.Length; ++i)
            {
                indices.Add(0);
                indices.Add(i);
                indices.Add((i+1) == vertices.Length ? 1 : (i+1));
            }
        }

        /// <summary>
        /// Sorts point in counter clockwise.
        /// Polygon points must be convex.
        /// </summary>
        public static void SortCounterClockwise(NativeArray<float2> points)
        {
            var sortPoint = GetSortPoint(points);
            points.Sort(new PointWithCenterClockwiseSort(sortPoint));
        }

        /// <summary>
        /// Returns centroid of convex polygon.
        /// Polygon points must be convex and sorted counter clockwise.
        /// Based on https://en.wikipedia.org/wiki/Centroid.
        /// </summary>
        public static float2 GetCentroid(NativeArray<float2> points)
        {
            var numPoints = points.Length;

            float A = 0;
            for (int pointIndex = 0; pointIndex < numPoints; ++pointIndex)
            {
                var point = points[pointIndex];
                var nextPoint = points[(pointIndex + 1) % numPoints];
                A += (point.x * nextPoint.y - nextPoint.x * point.y);
            }
            A *= 0.5f;

            float cx = 0;
            float cy = 0;
            for (int pointIndex = 0; pointIndex < numPoints; ++pointIndex)
            {
                var point = points[pointIndex];
                var nextPoint = points[(pointIndex + 1) % numPoints];

                var scaler = (point.x * nextPoint.y - nextPoint.x * point.y);

                cx += (point.x + nextPoint.x)*scaler;
                cy += (point.y + nextPoint.y)*scaler;
            }
            cx *= (1f / (6 * A));
            cy *= (1f / (6 * A));

            return new float2(cx, cy);
        }

        /// <summary>
        /// Returns area of convex polygon.
        /// Polygon points must be convex and sorted counter clockwise.
        /// </summary>
        public static float GetArea(NativeArray<float2> points)
        {
            float area = 0;
            for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
            {
                var polygonPoint = points[pointIndex];
                var nextPolygonPoint = points[(pointIndex + 1) % points.Length];

                area +=
                    (nextPolygonPoint.x - polygonPoint.x) *
                    (nextPolygonPoint.y + polygonPoint.y) / 2;
            }
            return -area;
        }

        /// <summary>
        /// Returns minimum distance between two convex polygons.
        /// Polygon points must be convex and sorted counter clockwise.
        /// </summary>
        public static float GetDistance(NativeArray<float2> pointsA, float scaleA, float2 offsetA, NativeArray<float2> pointsB, float scaleB, float2 offsetB)
        {
            return math.max(
                GetDistanceFromAToB(pointsA, scaleA, offsetA, pointsB, scaleB, offsetB),
                GetDistanceFromAToB(pointsB, scaleB, offsetB, pointsA, scaleA, offsetA));
        }

        static float GetDistanceFromAToB(NativeArray<float2> pointsA, float scaleA, float2 offsetA, NativeArray<float2> pointsB, float scaleB, float2 offsetB)
        {
            // This algorithm based on idea if two polygons projection on any 1D line never intersect.
            // Then they will not intersect in 2D space too.
            // Also it is enough to project on perpendicular lines to polygon.

            float distance = float.MinValue;
            for (int polygonPointIndex = 0; polygonPointIndex < pointsA.Length; ++polygonPointIndex)
            {
                float2 polygonPoint = pointsA[polygonPointIndex] * scaleA + offsetA;
                float2 nextPolygonPoint = pointsA[(polygonPointIndex + 1) % pointsA.Length] * scaleA + offsetA;

                var line = new Line(polygonPoint, nextPolygonPoint);

                var right = line.GetRight();

                Range projectedPolygonA = ProjectPolygonOnLine(pointsA, scaleA, offsetA, right);
                Range projectedPolygonB = ProjectPolygonOnLine(pointsB, scaleB, offsetB, right);

                float distanceBetweenProjectedPolygons = Range.Distance(projectedPolygonA, projectedPolygonB);
                distance = math.max(distance, distanceBetweenProjectedPolygons);
            }

            return distance;
        }

        static Range ProjectPolygonOnLine(NativeArray<float2> points, float scale, float2 offset, Line line)
        {
            var direction = line.Direction;
            var angle = -GetAngleBetweenVectors(direction, new float2(1, 0));

            float min = float.MaxValue;
            float max = float.MinValue;

            for (int pointIndex = 0; pointIndex < points.Length; ++pointIndex)
            {
                var point = points[pointIndex] * scale + offset;
                var projectedPoint = Rotate(point, angle);

                var nextPoint = points[(pointIndex + 1) % points.Length] * scale + offset;
                var nextProjectedPoint = Rotate(nextPoint, angle);

                min = math.min(min, projectedPoint.x);
                max = math.max(max, projectedPoint.x);
            }

            return new Range(min, max);
        }

        static float2 Rotate(float2 value, float angle)
        {
            var ca = math.cos(angle);
            var sa = math.sin(angle);
            return new float2(ca * value.x - sa * value.y, sa * value.x + ca * value.y);
        }

        static float GetAngleBetweenVectors(float2 a, float2 b)
        {
            return math.atan2(a.y, a.x);
        }

        static float2 GetSortPoint(NativeArray<float2> points)
        {
            float2 sum = 0;

            for (int i = 0; i < points.Length; ++i)
            {
                sum += points[i];
            }

            return sum / points.Length;
        }
    }
}
