using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Automatic voronoi normalization scope.
    /// Do not use it as field or property.
    /// </summary>
    public unsafe struct VoronoiNormalizationScope : IDisposable
    {
        void* m_VoronoiBuilder;
        void* m_VoronoiDiagram;
        void* m_Sites;
        Rect m_Bounds;
        float2 m_AspectScale;
        float2 m_Scale;
        float2 m_Offset;

        public VoronoiNormalizationScope(ref VoronoiBuilder voronoiBuilder, ref VoronoiDiagram voronoiDiagram, ref NativeArray<float2> sites)
        {
            if (!voronoiBuilder.IsCreated)
                throw new ArgumentException("Voronoi builder must be created.");
            if (!voronoiDiagram.IsCreated)
                throw new ArgumentException("Voronoi diagram must be created.");
            if (!sites.IsCreated)
                throw new ArgumentException("Voronoi sites must be created.");

            m_Bounds = voronoiDiagram.Bounds;

            m_AspectScale = new float2(m_Bounds.width / m_Bounds.height, 1);

            m_Scale = m_Bounds.size * 0.5f;
            m_Offset = m_Bounds.center;

            // To viewport
            for (int siteIndex = 0; siteIndex < sites.Length; ++siteIndex)
            {
                sites[siteIndex] = (sites[siteIndex] - m_Offset) / m_Scale;
                sites[siteIndex] *= m_AspectScale;
            }
            Rect bounds = new Rect(-1, -1, 2, 2);
            bounds.size *= (Vector2) m_AspectScale;
            bounds.center = Vector2.zero;
            voronoiBuilder.SetSites(sites);
            voronoiBuilder.Construct(ref voronoiDiagram, bounds);

            // Convert referenced to pointers so they could be restoed later on in dispose call
            m_VoronoiBuilder = UnsafeUtility.AddressOf(ref voronoiBuilder);
            m_VoronoiDiagram = UnsafeUtility.AddressOf(ref voronoiDiagram);
            m_Sites = UnsafeUtility.AddressOf(ref sites);
        }

        public void Dispose()
        {
            // https://docs.unity3d.com/Packages/com.unity.collections@1.2/changelog/CHANGELOG.html
            // [0.13.0] - 2020-08-26
            // Removed: UnsafeUtilityEx
            #if COLLECTIONS_0_13_0_OR_NEWER
            ref VoronoiBuilder voronoiBuilder = ref UnsafeUtility.AsRef<VoronoiBuilder>(m_VoronoiBuilder);
            ref VoronoiDiagram voronoiDiagram = ref UnsafeUtility.AsRef<VoronoiDiagram>(m_VoronoiDiagram);
            ref NativeArray<float2> sites = ref UnsafeUtility.AsRef<NativeArray<float2>>(m_Sites);
            #else
            ref VoronoiBuilder voronoiBuilder = ref UnsafeUtilityEx.AsRef<VoronoiBuilder>(m_VoronoiBuilder);
            ref VoronoiDiagram voronoiDiagram = ref UnsafeUtilityEx.AsRef<VoronoiDiagram>(m_VoronoiDiagram);
            ref NativeArray<float2> sites = ref UnsafeUtilityEx.AsRef<NativeArray<float2>>(m_Sites);
            #endif

            // To world space
            for (int siteIndex = 0; siteIndex < sites.Length; ++siteIndex)
            {
                sites[siteIndex] /= m_AspectScale;
                sites[siteIndex] = (sites[siteIndex] * m_Scale) + m_Offset;
            }
            voronoiBuilder.SetSites(sites);
            voronoiBuilder.Construct(ref voronoiDiagram, m_Bounds);
        }
    }
}
