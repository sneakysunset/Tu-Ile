#if HIGH_DEFINITION_RENDER_PIPELINE
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProjectDawn.SplitScreen.HighDefinition
{
    public sealed class SplitScreenCustomPass : CustomPass
    {
        protected override void Execute(CustomPassContext ctx)
        {
            if (ctx.hdCamera.camera.TryGetComponent<SplitScreenEffect>(out SplitScreenEffect splitScreen) && splitScreen.isActiveAndEnabled)
            {
                var cmd = ctx.cmd;
                CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer);
                splitScreen.UpdateCommandBuffer(ctx.cmd);
            }
        }
        
        // Currently 7.7.1 is not supported as there is bug with post processing ordering
        /*protected override void Execute(ScriptableRenderContext ctx, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
        {
            if (hdCamera.camera.TryGetComponent<SplitScreenEffect>(out SplitScreenEffect splitScreen) && splitScreen.isActiveAndEnabled)
            {
                SetCameraRenderTarget(cmd);
                splitScreen.UpdateCommandBuffer(cmd);
            }
        }*/
    }
}
#endif