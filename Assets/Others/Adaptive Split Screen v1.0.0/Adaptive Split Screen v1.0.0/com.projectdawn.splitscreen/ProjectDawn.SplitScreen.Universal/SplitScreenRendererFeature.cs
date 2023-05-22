#if UNIVERSAL_RENDER_PIPELINE
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectDawn.SplitScreen.Universal
{
    [DisallowMultipleComponent]
    public class SplitScreenRendererFeature : ScriptableRendererFeature
    {
        public static bool AnyCreated { private set; get; }

        public RenderPassEvent Event = RenderPassEvent.BeforeRenderingPostProcessing;

        class SplitScreenRenderPass : ScriptableRenderPass
        {
            public SplitScreenRenderPass(RenderPassEvent ev)
            {
                renderPassEvent = ev;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var camera = renderingData.cameraData.camera;

                if (!camera.TryGetComponent<SplitScreenEffect>(out SplitScreenEffect splitScreen))
                    return;

                if (!splitScreen.isActiveAndEnabled)
                    return;

                SplitScreenRendererFeature.AnyCreated = true;

                var cmd = splitScreen.GetCommandBuffer();
                context.ExecuteCommandBuffer(cmd);
            }
        }

        SplitScreenRenderPass m_SplitScreenPass;

        public override void Create()
        {
            m_SplitScreenPass = new SplitScreenRenderPass(Event);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_SplitScreenPass);
        }
    }
}
#endif

