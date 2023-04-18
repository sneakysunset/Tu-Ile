#if UNIVERSAL_RENDER_PIPELINE
using UnityEngine.Rendering.Universal;
using UnityEditor;
using ProjectDawn.SplitScreen.Editor;

namespace ProjectDawn.SplitScreen.Universal.Editor
{
    [CustomEditorForRenderPipeline(typeof(SplitScreenEffect), typeof(UniversalRenderPipelineAsset))]
    [CanEditMultipleObjects]
    public class UniversalSplitScreenEffectEditor : SplitScreenEffectEditor
    {
        protected override void OnValidate()
        {
            if (!SplitScreenRendererFeature.AnyCreated)
            {
                // For some reason help box get null reference exception on first frame
                try
                {
                    EditorGUILayout.HelpBox("UniversalRenderPipeline is used, but SplitScreenRendererFeature is not detected in Renderers. Go to your URP asset renderer and add SplitScreenRendererFeature.", MessageType.Error);
                }
                catch
                {
                }
            }
        }
    }
}
#endif