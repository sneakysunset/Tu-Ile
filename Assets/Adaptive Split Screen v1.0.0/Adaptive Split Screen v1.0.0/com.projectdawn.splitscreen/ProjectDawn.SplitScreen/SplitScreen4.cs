using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    [BurstCompile]
    public struct SplitScreen4BalanceJob : IJob
    {
        public SplitScreen4 SplitScreen;
        public Balancing Balancing;

        public void Execute()
        {
            SplitScreen.Balance(Balancing);
        }
    }

    /// <summary>
    /// Split screen implementation for 4 players.
    /// </summary>
    public struct SplitScreen4 : ISplitScreen, IDisposable
    {
        VoronoiBuilder m_VoronoiBuilder;
        VoronoiDiagram m_VoronoiDiagram;
        float3 m_Position0;
        float3 m_Position1;
        float3 m_Position2;
        float3 m_Position3;
        float m_Scale;
        NativeArray<float2> m_Sites;

        public NativeArray<float2> Sites => m_Sites;
        public VoronoiBuilder VoronoiBuilder => m_VoronoiBuilder;
        public VoronoiDiagram VoronoiDiagram => m_VoronoiDiagram;
        public bool IsCreated => m_VoronoiDiagram.IsCreated && m_Sites.IsCreated;

        public SplitScreen4(Allocator allocator = Allocator.Temp)
        {
            m_Position0 = float3.zero;
            m_Position1 = float3.zero;
            m_Position2 = float3.zero;
            m_Position3 = float3.zero;
            m_Scale = 0;
            m_VoronoiBuilder = new VoronoiBuilder(allocator);
            m_VoronoiDiagram = new VoronoiDiagram(allocator);
            m_Sites = new NativeArray<float2>(4, allocator);
        }

        /// <summary>
        /// Recreate split screen with set of player positions.
        /// </summary>
        public void Reset(float3 w0, float3 w1, float3 w2, float3 w3, float scale = 2, float aspectRatio = 1, float BoundsRadius = 0.3f)
        {
            CheckIsCreated();

            m_Position0 = w0;
            m_Position1 = w1;
            m_Position2 = w2;
            m_Position3 = w3;

            m_Sites[0] = w0.xy;
            m_Sites[1] = w1.xy;
            m_Sites[2] = w2.xy;
            m_Sites[3] = w3.xy;

            var localBounds = VoronoiBounds.CreateFromSites(aspectRatio, m_Sites, BoundsRadius);
            m_Scale = scale / localBounds.width;

            m_VoronoiBuilder.SetSites(w0.xy, w1.xy, w2.xy, w3.xy);
            m_VoronoiBuilder.Construct(ref m_VoronoiDiagram, localBounds);
        }

        /// <summary>
        /// Balance screen regions to similar area.
        /// </summary>
        public void Balance(in Balancing balanceSettings)
        {
            CheckIsCreated();
            var normalization = new VoronoiNormalizationScope(ref m_VoronoiBuilder, ref m_VoronoiDiagram, ref m_Sites);
            {
                float4 position = float4.zero;
                if (balanceSettings.Enabled)
                {
                    if (balanceSettings.RelaxationIterations != 0)
                        FloydRelaxation.Execute(ref m_VoronoiBuilder, ref m_VoronoiDiagram, ref m_Sites, balanceSettings.RelaxationIterations);
                    UniformTransformRelaxation.Execute(ref m_VoronoiBuilder, ref m_VoronoiDiagram, ref m_Sites, ref position, 200, 0.009f);
                }
            }
            normalization.Dispose(); // Burst do not support "using"
        }

        /// <summary>
        /// Returns the value of how much the screens are balanced. Where 0 means fully balanced.
        /// </summary>
        public float GetBalanceError()
        {
            CheckIsCreated();
            float area = (m_VoronoiDiagram.Bounds.width * m_VoronoiDiagram.Bounds.height);
            float4 values = new float4(
                m_VoronoiDiagram.GetRegionArea(m_VoronoiDiagram.Regions[0]),
                m_VoronoiDiagram.GetRegionArea(m_VoronoiDiagram.Regions[1]),
                m_VoronoiDiagram.GetRegionArea(m_VoronoiDiagram.Regions[2]),
                m_VoronoiDiagram.GetRegionArea(m_VoronoiDiagram.Regions[3]));
            float4 diff = (values / area) - 1f/4f;
            return math.sqrt(math.dot(diff, diff));
        }

        public void UpdatePositions(float3 p0, float3 p1, float3 p2, float3 p3)
        {
            m_Position0 = p0;
            m_Position1 = p1;
            m_Position2 = p2;
            m_Position3 = p3;
        }

        /// <summary>
        /// Creates players screen regions.
        /// </summary>
        public void CreateScreens(in Translating translating, ref ScreenRegions screenRegions)
        {
            CheckIsCreated();

            var bounds = m_VoronoiDiagram.Bounds;

            VoronoiRegion S0 = m_VoronoiDiagram.Regions[0];
            VoronoiRegion S1 = m_VoronoiDiagram.Regions[1];
            VoronoiRegion S2 = m_VoronoiDiagram.Regions[2];
            VoronoiRegion S3 = m_VoronoiDiagram.Regions[3];

            float2 center0 = m_VoronoiDiagram.GetCentroid(S0);
            float2 center1 = m_VoronoiDiagram.GetCentroid(S1);
            float2 center2 = m_VoronoiDiagram.GetCentroid(S2);
            float2 center3 = m_VoronoiDiagram.GetCentroid(S3);

            float2 player0 = m_Position0.xy;
            float2 player1 = m_Position1.xy;
            float2 player2 = m_Position2.xy;
            float2 player3 = m_Position3.xy;

            // Calculate distances between regions
            float distance01;
            float distance02;
            float distance12;
            float distance03;
            float distance32;
            float distance31;
            if (translating.BlendShape == BlendShape.Region)
            {
                distance01 = m_VoronoiDiagram.GetDistanceBetweenRegions(S0, m_Scale, -center0 * m_Scale + player0, S1, m_Scale, -center1 * m_Scale + player1);
                distance02 = m_VoronoiDiagram.GetDistanceBetweenRegions(S0, m_Scale, -center0 * m_Scale + player0, S2, m_Scale, -center2 * m_Scale + player2);
                distance12 = m_VoronoiDiagram.GetDistanceBetweenRegions(S1, m_Scale, -center1 * m_Scale + player1, S2, m_Scale, -center2 * m_Scale + player2);
                distance03 = m_VoronoiDiagram.GetDistanceBetweenRegions(S0, m_Scale, -center0 * m_Scale + player0, S3, m_Scale, -center3 * m_Scale + player3);
                distance32 = m_VoronoiDiagram.GetDistanceBetweenRegions(S3, m_Scale, -center3 * m_Scale + player3, S2, m_Scale, -center2 * m_Scale + player2);
                distance31 = m_VoronoiDiagram.GetDistanceBetweenRegions(S3, m_Scale, -center3 * m_Scale + player3, S1, m_Scale, -center1 * m_Scale + player1);
            }
            else
            {
                var circle0 = new Circle(player0, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S0));
                var circle1 = new Circle(player1, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S1));
                var circle2 = new Circle(player2, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S2));
                var circle3 = new Circle(player3, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S3));

                distance01 = Circle.GetDistance(circle0, circle1);
                distance02 = Circle.GetDistance(circle0, circle2);
                distance12 = Circle.GetDistance(circle1, circle2);
                distance03 = Circle.GetDistance(circle0, circle3);
                distance32 = Circle.GetDistance(circle3, circle2);
                distance31 = Circle.GetDistance(circle3, circle1);
            }

            float3 cc0;
            float3 cc1;
            float3 cc2;
            float3 cc3;
            switch (translating.Centering)
            {
                case CameraCentering.PlayerCentered:
                    cc0 = m_Position0;
                    cc1 = m_Position1;
                    cc2 = m_Position2;
                    cc3 = m_Position3;
                    break;
                case CameraCentering.ScreenCentered:
                    {
                        float2 centerOffset0 = center0 - new float2(bounds.center);
                        centerOffset0 *= m_Scale;

                        float2 centerOffset1 = center1 - new float2(bounds.center);
                        centerOffset1 *= m_Scale;

                        float2 centerOffset2 = center2 - new float2(bounds.center);
                        centerOffset2 *= m_Scale;

                        float2 centerOffset3 = center3 - new float2(bounds.center);
                        centerOffset3 *= m_Scale;

                        cc0 = new float3(player0 - centerOffset0, m_Position0.z);
                        cc1 = new float3(player1 - centerOffset1, m_Position1.z);
                        cc2 = new float3(player2 - centerOffset2, m_Position2.z);
                        cc3 = new float3(player3 - centerOffset3, m_Position3.z);
                        break;
                    }
                case CameraCentering.None:
                    {
                        float2 centerOffset0 = player0 - new float2(bounds.center);
                        centerOffset0 *= m_Scale;

                        float2 centerOffset1 = player1 - new float2(bounds.center);
                        centerOffset1 *= m_Scale;

                        float2 centerOffset2 = player2 - new float2(bounds.center);
                        centerOffset2 *= m_Scale;

                        float2 centerOffset3 = player3 - new float2(bounds.center);
                        centerOffset3 *= m_Scale;

                        cc0 = new float3(player0 - centerOffset0, m_Position0.z);
                        cc1 = new float3(player1 - centerOffset1, m_Position1.z);
                        cc2 = new float3(player2 - centerOffset2, m_Position2.z);
                        cc3 = new float3(player3 - centerOffset3, m_Position3.z);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            // Calculate blend values
            var blend = translating.Blend;
            float start = blend;
            float end = 0;
            float blend01 = Blend.LinearSafe(distance01, start, end);
            float blend12 = Blend.LinearSafe(distance12, start, end);
            float blend02 = Blend.LinearSafe(distance02, start, end);
            float blend03 = Blend.LinearSafe(distance03, start, end);
            float blend31 = Blend.LinearSafe(distance31, start, end);
            float blend32 = Blend.LinearSafe(distance32, start, end);
            Blend.ForceToQuad(ref blend01, ref blend12, ref blend02, ref blend03, ref blend31, ref blend32);

            // Blend world positions
            Blend.BlendQuad(
                cc0, cc1, cc2, cc3,
                blend01, blend12, blend02, blend03, blend31, blend32,
                out float3 p0, out float3 p1, out float3 p2, out float3 p3);

            float2 boundsOffset = -new float2(bounds.center);
            float2 boundsScale = 2f / new float2(bounds.size);

            // Blend uv positions
            float2 u0;
            float2 u1;
            float2 u2;
            float2 u3;
            switch (translating.Centering)
            {
                case CameraCentering.PlayerCentered:
                    float2 au = (center0 + boundsOffset) * boundsScale;
                    float2 bu = (center1 + boundsOffset) * boundsScale;
                    float2 cu = (center2 + boundsOffset) * boundsScale;
                    float2 du = (center3 + boundsOffset) * boundsScale;

                    Blend.BlendQuad(
                        au, bu, cu, du,
                        blend01, blend12, blend02, blend03, blend31, blend32,
                        out u0, out u1, out u2, out u3);
                        break;
                case CameraCentering.ScreenCentered:
                case CameraCentering.None:
                    u0 = 0;
                    u1 = 0;
                    u2 = 0;
                    u3 = 0;
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            // Update screen regions
            screenRegions.Clear();
            screenRegions.AddRegion(m_VoronoiDiagram, S0, p0, u0);
            screenRegions.AddRegion(m_VoronoiDiagram, S1, p1, u1);
            screenRegions.AddRegion(m_VoronoiDiagram, S2, p2, u2);
            screenRegions.AddRegion(m_VoronoiDiagram, S3, p3, u3);
            screenRegions.AddSplit(m_VoronoiDiagram, S0, S1, blend01);
            screenRegions.AddSplit(m_VoronoiDiagram, S1, S2, blend12);
            screenRegions.AddSplit(m_VoronoiDiagram, S0, S2, blend02);
            screenRegions.AddSplit(m_VoronoiDiagram, S0, S3, blend03);
            screenRegions.AddSplit(m_VoronoiDiagram, S3, S1, blend31);
            screenRegions.AddSplit(m_VoronoiDiagram, S3, S2, blend32);
        }

        public void Dispose()
        {
            CheckIsCreated();
            m_VoronoiBuilder.Dispose();
            m_VoronoiDiagram.Dispose();
            m_Sites.Dispose();
        }

        public void DrawDelaunayDual()
        {
            CheckIsCreated();
            var triangles = new NativeList<Triangle>(Allocator.Temp);
            DelaunayDual.GetTriangles(m_Sites[0], m_Sites[1], m_Sites[2], m_Sites[3], ref triangles);
            foreach (var triangle in triangles)
                triangle.Draw(Color.black);
            triangles.Dispose();
        }

        public void DrawRegions(BlendShape blendShape)
        {
            CheckIsCreated();

            var regions = m_VoronoiDiagram.Regions;

            VoronoiRegion S0 = regions[0];
            VoronoiRegion S1 = regions[1];
            VoronoiRegion S2 = regions[2];
            VoronoiRegion S3 = regions[3];

            float2 center0 = m_VoronoiDiagram.GetCentroid(S0);
            float2 center1 = m_VoronoiDiagram.GetCentroid(S1);
            float2 center2 = m_VoronoiDiagram.GetCentroid(S2);
            float2 center3 = m_VoronoiDiagram.GetCentroid(S3);

            float2 player0 = m_Position0.xy;
            float2 player1 = m_Position1.xy;
            float2 player2 = m_Position2.xy;
            float2 player3 = m_Position3.xy;

            m_VoronoiDiagram.DrawRegion(S0, PlayerColor.PlayerA);
            m_VoronoiDiagram.DrawRegion(S1, PlayerColor.PlayerB);
            m_VoronoiDiagram.DrawRegion(S2, PlayerColor.PlayerC);
            m_VoronoiDiagram.DrawRegion(S3, PlayerColor.PlayerD);

            // Draw centers
            Gizmos.DrawPoint(center0, 0.003f * m_VoronoiDiagram.Bounds.width, Color.white);
            Gizmos.DrawPoint(center1, 0.003f * m_VoronoiDiagram.Bounds.width, Color.white);
            Gizmos.DrawPoint(center2, 0.003f * m_VoronoiDiagram.Bounds.width, Color.white);
            Gizmos.DrawPoint(center3, 0.003f * m_VoronoiDiagram.Bounds.width, Color.white);

            // Draw screen regions
            if (blendShape == BlendShape.Region)
            {
                m_VoronoiDiagram.DrawRegion(S0, m_Scale, -center0 * m_Scale + player0, new Color32(255, 255, 255, 128));
                m_VoronoiDiagram.DrawRegion(S1, m_Scale, -center1 * m_Scale + player1, new Color32(255, 255, 255, 128));
                m_VoronoiDiagram.DrawRegion(S2, m_Scale, -center2 * m_Scale + player2, new Color32(255, 255, 255, 128));
                m_VoronoiDiagram.DrawRegion(S3, m_Scale, -center3 * m_Scale + player3, new Color32(255, 255, 255, 128));
            }
            else
            {
                Gizmos.DrawPoint(player0, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S0), new Color32(255, 255, 255, 128));
                Gizmos.DrawPoint(player1, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S1), new Color32(255, 255, 255, 128));
                Gizmos.DrawPoint(player2, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S2), new Color32(255, 255, 255, 128));
                Gizmos.DrawPoint(player3, m_Scale * m_VoronoiDiagram.GetRegionInscribedCircleRadius(S3), new Color32(255, 255, 255, 128));
            }

            // Draw inscribed circles
            Gizmos.DrawPointWire(center0, m_VoronoiDiagram.GetRegionInscribedCircleRadius(S0), Color.white);
            Gizmos.DrawPointWire(center1, m_VoronoiDiagram.GetRegionInscribedCircleRadius(S1), Color.white);
            Gizmos.DrawPointWire(center2, m_VoronoiDiagram.GetRegionInscribedCircleRadius(S2), Color.white);
            Gizmos.DrawPointWire(center3, m_VoronoiDiagram.GetRegionInscribedCircleRadius(S3), Color.white);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIsCreated()
        {
            if (!IsCreated)
                throw new Exception("Split screen must be created.");
        }
    }
}
