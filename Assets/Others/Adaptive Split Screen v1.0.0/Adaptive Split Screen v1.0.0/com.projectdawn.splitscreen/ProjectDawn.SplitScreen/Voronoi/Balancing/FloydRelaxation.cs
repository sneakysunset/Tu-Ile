using System;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class FloydRelaxation
    {
        /// <summary>
        /// Executes floyd relaxation that every iteration moves sites to their centroids.
        /// Based on https://en.wikipedia.org/wiki/Lloyd%27s_algorithm.
        /// </summary>
        public static void Execute(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, ref NativeArray<float2> sites, int numIterations)
        {
            var regions = voronoiDiagram.Regions;
            for (int i = 0; i < numIterations; ++i)
            {
                for (int regionIndex = 0; regionIndex < regions.Length; ++regionIndex)
                {
                    VoronoiRegion region = regions[regionIndex];
                    float2 centroid = voronoiDiagram.GetCentroid(region);
                    sites[regionIndex] = centroid;
                }

                voronoiBuilder.SetSites(sites);
                voronoiBuilder.Construct(ref voronoiDiagram, voronoiDiagram.Bounds);
            }
        }
    }
}
