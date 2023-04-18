#if HIGH_DEFINITION_RENDER_PIPELINE
using System;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using ProjectDawn.SplitScreen.Editor;

namespace ProjectDawn.SplitScreen.HighDefinition.Editor
{
    [CustomEditorForRenderPipeline(typeof(SplitScreenEffect), typeof(HDRenderPipelineAsset))]
    [CanEditMultipleObjects]
    class HDSplitScreenEffectEditor : SplitScreenEffectEditor
    {
        protected override void OnValidate()
        {
            var splitScreen = target as SplitScreenEffect;
            if (splitScreen.TryGetComponent<CustomPassVolume>(out CustomPassVolume customPassVolume))
            {
                if (!TryGetSplitScreenCustomPass(customPassVolume, out SplitScreenCustomPass splitScreenCustomPass))
                {
                    SplitScreenEffectEditor.DrawFixMeBox("CustomPassVolume does not contains SplitScreenCustomPass.", MessageType.Error, () =>
                    {
                        customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
                        customPassVolume.AddPassOfType(typeof(SplitScreenCustomPass));
                    });
                }
            }
            else
            {
                SplitScreenEffectEditor.DrawFixMeBox("HDRenderPipeline is used, but CustomPassVolume is not added into this game object.", MessageType.Error, () =>
                {
                    customPassVolume = splitScreen.gameObject.AddComponent<CustomPassVolume>();
                    customPassVolume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
                    customPassVolume.AddPassOfType(typeof(SplitScreenCustomPass));
                });
            }
        }

        static bool TryGetSplitScreenCustomPass(CustomPassVolume customPassVolume, out SplitScreenCustomPass splitScreenCustomPass)
        {
            foreach (var pass in customPassVolume.customPasses)
            {
                if (pass is SplitScreenCustomPass)
                {
                    splitScreenCustomPass = pass as SplitScreenCustomPass;
                    return true;
                }
            }
            splitScreenCustomPass = null;
            return false;
        }
    }
}
#endif