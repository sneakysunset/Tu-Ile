using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.SplitScreen
{
    public class VoronoiDraw : MonoBehaviour
    {
        public Transform[] Targets;
        public Rect Rect;

        void OnDrawGizmos()
        {
            if (Targets == null)
                return;

            using (var builder = new VoronoiBuilder(Allocator.Temp))
            {
                // Gather sites
                var sites = new NativeList<float2>(Targets.Length, Allocator.Temp);
                foreach (var target in Targets)
                {
                    if (target)
                        sites.Add(((float3) target.transform.position).xy);
                }
                builder.SetSites(sites);

                // Create voronoi
                var voronoiDiagram = new VoronoiDiagram(Allocator.Temp);
                builder.Construct(ref voronoiDiagram, Rect);

                Gizmos.DrawBounds(Rect, 1, Color.white);

                // Draw voronoi
                foreach (var region in voronoiDiagram.Regions)
                    Gizmos.DrawConvexPolygon(voronoiDiagram.GetRegionPoints(region), 1, 0, PlayerColor.Players[region.Index]);

                // Cleanup
                sites.Dispose();
                voronoiDiagram.Dispose();
            }
        }
    }
}
