//#define URP_10_0_0_OR_NEWER
//#define UNITY_2021_2_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScreenSpaceCavityCurvature.Universal
{
    [ExecuteInEditMode, VolumeComponentMenu("ScreenSpaceCavityCurvature")]
    public class SSCC : VolumeComponent, IPostProcessComponent
    {

        public enum PerPixelNormals
        {
            ReconstructedFromDepth,
#if URP_10_0_0_OR_NEWER
            Camera
#else
            Camera_URP_VER_TOO_LOW
#endif
        }
        public enum DebugMode { Disabled, EffectOnly, ViewNormals }
        public enum CavitySamples { Low6, Medium8, High12 }
        public enum OutputEffectTo
        {
            Screen,
#if URP_10_0_0_OR_NEWER
            [InspectorName("_SSCCTexture in shaders")] _SSCCTexture
#else
            [InspectorName("_SSCCTexture (URP 10+)")] _SSCCTexture
#endif
        }

        [Serializable] public sealed class DebugModeParameter : VolumeParameter<DebugMode> { public DebugModeParameter(DebugMode value, bool overrideState = false) : base(value, overrideState) { } }
        [Serializable] public sealed class GetNormalsFromParameter : VolumeParameter<PerPixelNormals> { public GetNormalsFromParameter(PerPixelNormals value, bool overrideState = false) : base(value, overrideState) { } }
        [Serializable] public sealed class CavitySamplesParameter : VolumeParameter<CavitySamples> { public CavitySamplesParameter(CavitySamples value, bool overrideState = false) : base(value, overrideState) { } }
        [Serializable] public sealed class OutputParameter : VolumeParameter<OutputEffectTo> { public OutputParameter(OutputEffectTo value, bool overrideState = false) : base(value, overrideState) { } }

        //
        [Header("(Make sure Post Processing and Depth Texture are enabled.)")]
        [Tooltip("Lerps the whole effect from 0 to 1.")] public ClampedFloatParameter effectIntensity = new ClampedFloatParameter(1f, 0f, 1f);
        [Tooltip("Divides effect intensity by (depth * distanceFade).\nZero means effect doesn't fade with distance.")] public ClampedFloatParameter distanceFade = new ClampedFloatParameter(0f, 0f, 1f);

        [Space(6)]

        [Tooltip("The radius of curvature calculations in pixels.")] public ClampedIntParameter curvaturePixelRadius = new ClampedIntParameter(2, 0, 4);
        [Tooltip("How bright does curvature get.")] public ClampedFloatParameter curvatureBrights = new ClampedFloatParameter(2f, 0f, 5f);
        [Tooltip("How dark does curvature get.")] public ClampedFloatParameter curvatureDarks = new ClampedFloatParameter(3f, 0f, 5f);

        [Space(6)]

        [Tooltip("The amount of samples used for cavity calculation.")] public CavitySamplesParameter cavitySamples = new CavitySamplesParameter(CavitySamples.High12);
        [Tooltip("True: uses pow() blending to make colors more saturated in bright/dark areas of cavity.\n\nFalse: uses additive blending.")] public BoolParameter saturateCavity = new BoolParameter(true);
        [Tooltip("The radius of cavity calculations in world units.")] public ClampedFloatParameter cavityRadius = new ClampedFloatParameter(0.25f, 0f, 0.5f);
        [Tooltip("How bright does cavity get.")] public ClampedFloatParameter cavityBrights = new ClampedFloatParameter(3f, 0f, 5f);
        [Tooltip("How dark does cavity get.")] public ClampedFloatParameter cavityDarks = new ClampedFloatParameter(2f, 0f, 5f);

        [Space(6)]

        [Tooltip("Where to get normals from.")]
#if URP_10_0_0_OR_NEWER
        public GetNormalsFromParameter normalsSource = new GetNormalsFromParameter(PerPixelNormals.Camera);
#else
        public GetNormalsFromParameter normalsSource = new GetNormalsFromParameter(PerPixelNormals.ReconstructedFromDepth);
#endif
        [Tooltip("May be useful to check what objects contribute normals, as objects that do not contribute their normals will not contribute to the effect.")] public DebugModeParameter debugMode = new DebugModeParameter(DebugMode.Disabled);

        [Space(6)]

        [Tooltip("Screen: Applies the effect over the entire screen.\n\n_SSCCTexture: Instead of writing the effect to the screen, will write the effect into a global shader texture named _SSCCTexture, so you can sample it selectively in your shaders and exclude certain objects from receiving outlines etc. See \"Output To Texture Examples\" folder for example shaders.")] public OutputParameter output = new OutputParameter(OutputEffectTo.Screen);
        //

        public bool IsActive() => true;

        public bool IsTileCompatible() => true;
    }
}
