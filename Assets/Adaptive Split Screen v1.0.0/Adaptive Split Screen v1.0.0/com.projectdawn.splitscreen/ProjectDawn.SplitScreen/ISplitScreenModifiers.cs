using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Rendering;

namespace ProjectDawn.SplitScreen
{
    public interface ISplitScreenTargetPosition
    {
        float3 OnSplitScreenTargetPosition(int screenIndex, float3 positionWS);
    }

    public interface ISplitScreenTranslatingPosition
    {
        float3 OnSplitScreenTranslatingPosition(int screenIndex, float3 positionWS);
    }

    public interface ISplitScreenCommandBuffer
    {
        void OnSplitScreenCommandBuffer(CommandBuffer commandBuffer, in ScreenRegions screenRegions);
    }

    public interface ISplitScreenBalancing
    {
        void OnSplitScreenBalancing(VoronoiBuilder voronoiBuilder, VoronoiDiagram voronoiDiagram, NativeArray<float2> sites, in Balancing balancing);
    }
}
