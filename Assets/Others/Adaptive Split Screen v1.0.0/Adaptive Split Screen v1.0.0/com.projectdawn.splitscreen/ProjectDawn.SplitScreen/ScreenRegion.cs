using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    public struct ScreenRegion
    {
        public RangeInt Range;
        public float2 Center;
        public float2 Uv;
        public float3 Position;
    }

    public struct ScreenSplit
    {
        public Line Line;
        public float Blend;
    }

    /// <summary>
    /// Regions in screen space.
    /// </summary>
    public struct ScreenRegions : IDisposable
    {
        public NativeList<ScreenRegion> Regions;
        public NativeList<float2> Points;
        public NativeList<ScreenSplit> Splits;

        public bool IsCreated => Regions.IsCreated && Points.IsCreated && Splits.IsCreated;
        public int Length => Regions.Length;

        public ScreenRegions(Allocator allocator = Allocator.Temp)
        {
            Regions = new NativeList<ScreenRegion>(allocator);
            Points = new NativeList<float2>(allocator);
            Splits = new NativeList<ScreenSplit>(allocator);
        }

        public void AddRegion(in VoronoiDiagram voronoiDiagram, in VoronoiRegion region, float3 position, float2 uv)
        {
            var points = voronoiDiagram.GetRegionPoints(region);
            float2 center = voronoiDiagram.GetCentroid(region);

            Rect bounds = voronoiDiagram.Bounds;
            float2 offset = -new float2(bounds.center);
            float2 scale = 2f / new float2(bounds.size);

            Regions.Add(new ScreenRegion
            {
                Range = new RangeInt(Points.Length, points.Length),
                Center = (center + offset) * scale,
                Position = position,
                Uv = uv,
            });

            for (int i = 0; i < points.Length; ++i)
            {
                var point = points[i];

                // To screen space
                point = (point + offset) * scale;

                Points.Add(point);
            }
        }

        public void AddSplit(in VoronoiDiagram voronoiDiagram, in VoronoiRegion regionA, in VoronoiRegion regionB, float blend)
        {
            if (blend == 1)
                return;

            if (voronoiDiagram.TryFindEdgeBetweenRegions(regionA, regionB, out Line edge))
            {
                Rect bounds = voronoiDiagram.Bounds;
                float2 offset = -new float2(bounds.center);
                float2 scale = 2f / new float2(bounds.size);

                Splits.Add(new ScreenSplit
                {
                    Line = new Line((edge.From + offset) * scale, (edge.To + offset) * scale),
                    Blend = 1 - blend,
                });
            }
        }

        public void Clear()
        {
            Regions.Clear();
            Points.Clear();
            Splits.Clear();
        }

        public ScreenRegion GetRegion(int index)
        {
            CheckIndex(index);
            return Regions[index];
        }

        public NativeSlice<float2> GetRegionPoints(int index)
        {
            CheckIndex(index);
            var region = Regions[index];
            return Points.AsArray().Slice(region.Range.start, region.Range.length);
        }

        public Rect GetRegionRect(int index)
        {
            CheckIndex(index);
            var uv = GetRegion(index).Uv;
            var points = GetRegionPoints(index);
            float2 min = float.MaxValue;
            float2 max = float.MinValue;
            for (int i = 0; i < points.Length; ++i)
            {
                var point = (points[i] - uv) * 0.5f + 0.5f;
                min = math.min(min, point);
                max = math.max(max, point);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public void Dispose()
        {
            Regions.Dispose();
            Points.Dispose();
            Splits.Dispose();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIndex(int index)
        {
            if (!IsCreated)
                throw new Exception("Screen regions must be created.");
            if (index < 0 || index >= Regions.Length)
                throw new ArgumentOutOfRangeException($"Region index {index} is out of range.");
        }
    }
}
