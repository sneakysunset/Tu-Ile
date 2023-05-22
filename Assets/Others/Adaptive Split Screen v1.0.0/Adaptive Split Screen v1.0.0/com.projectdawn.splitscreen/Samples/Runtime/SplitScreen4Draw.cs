using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.SplitScreen
{
    public class SplitScreen4Draw : MonoBehaviour
    {
        public Transform TargetA;
        public Transform TargetB;
        public Transform TargetC;

        void OnDrawGizmos()
        {
            if (TargetA == null || TargetB == null || TargetC == null)
                return;

            // Create split screen
            var splitScreen = new SplitScreen3(Allocator.TempJob);
            splitScreen.Reset(TargetA.transform.position, TargetB.transform.position, TargetC.transform.position, 2, 1, 0.2f);

            // Creates screen regions
            var screenRegions = new ScreenRegions(Allocator.TempJob);
            splitScreen.CreateScreens(Translating.Default, ref screenRegions);

            // Draw screen regions
            for (int screenRegionIndex = 0; screenRegionIndex < screenRegions.Length; ++screenRegionIndex)
            {
                float3 cameraPositionWS = screenRegions.Regions[screenRegionIndex].Position;
                Gizmos.DrawConvexPolygon(screenRegions.GetRegionPoints(screenRegionIndex), 1, cameraPositionWS.xy, PlayerColor.Players[screenRegionIndex]);
            }

            // Cleanup
            splitScreen.Dispose();
            screenRegions.Dispose();
        }
    }
}
