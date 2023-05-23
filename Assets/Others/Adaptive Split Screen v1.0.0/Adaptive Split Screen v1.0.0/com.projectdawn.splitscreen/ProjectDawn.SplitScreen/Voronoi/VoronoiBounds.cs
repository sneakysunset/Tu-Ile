using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.SplitScreen
{
    public static class VoronoiBounds
    {
        /// <summary>
        /// Creates bounds that encapsulates all sites.
        /// </summary>
        public static Rect CreateFromSites(float aspectRatio, NativeArray<float2> sites, float radius)
        {
            float2 min = float.MaxValue;
            float2 max = float.MinValue;

            for (int siteIndex = 0; siteIndex < sites.Length; ++siteIndex)
            {
                float2 site = sites[siteIndex];
                min = math.min(min, site);
                max = math.max(max, site);
            }

            min -= radius;
            max += radius;

            var rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

            float rectAspectRatio = rect.width / rect.height;
            float scale = rectAspectRatio / aspectRatio;

            var center = rect.center;

            if (rectAspectRatio < aspectRatio)
            {
                rect.width *= (aspectRatio / rectAspectRatio);
            }
            else
            {
                rect.height *= (rectAspectRatio / aspectRatio);
            }

            rect.center = center;

            return rect;
        }
    }
}
