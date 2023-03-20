//#define URP_10_0_0_OR_NEWER
//#define UNITY_2021_2_OR_NEWER

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ScreenSpaceCavityCurvature.Universal
{

    public class SSCCRendererFeature : ScriptableRendererFeature
    {
        private class SSCCRenderPass : ScriptableRenderPass
        {
            void CheckParameters()
            {
#if !URP_10_0_0_OR_NEWER
                if (sscc.normalsSource.value == SSCC.PerPixelNormals.Camera_URP_VER_TOO_LOW)
                {
                    sscc.normalsSource.value = SSCC.PerPixelNormals.ReconstructedFromDepth;
                    Debug.LogWarning("URP version too low for Camera based normals, only available in URP 10+ (Unity 2020+).");
                }
                if (sscc.output.value == SSCC.OutputEffectTo._SSCCTexture)
                {
                    sscc.output.value = SSCC.OutputEffectTo.Screen;
                    Debug.LogWarning("URP version too low for texture output mode, only available in URP 10+ (Unity 2020+).");
                }
#endif
            }

            SSCC.OutputEffectTo Output => sscc.debugMode.value != SSCC.DebugMode.Disabled ? SSCC.OutputEffectTo.Screen : sscc.output.value;

            public SSCC sscc;

            static class Pass
            {
                public const int Copy = 0;
                public const int Composite = 1;
            }

            static class ShaderProperties
            {
                public static int mainTex = Shader.PropertyToID("_MainTex");
                public static int tempTex = Shader.PropertyToID("_TempTex");
                public static int uvTransform = Shader.PropertyToID("_UVTransform");
                public static int inputTexelSize = Shader.PropertyToID("_Input_TexelSize");
                public static int uvToView = Shader.PropertyToID("_UVToView");
                public static int worldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");

                public static int effectIntensity = Shader.PropertyToID("_EffectIntensity");
                public static int distanceFade = Shader.PropertyToID("_DistanceFade");

                public static int curvaturePixelRadius = Shader.PropertyToID("_CurvaturePixelRadius");
                public static int curvatureRidge = Shader.PropertyToID("_CurvatureBrights");
                public static int curvatureValley = Shader.PropertyToID("_CurvatureDarks");

                public static int cavityWorldRadius = Shader.PropertyToID("_CavityWorldRadius");
                public static int cavityRidge = Shader.PropertyToID("_CavityBrights");
                public static int cavityValley = Shader.PropertyToID("_CavityDarks");

                public static int globalSSCCTexture = Shader.PropertyToID("_SSCCTexture");
            }

            Material material { get; set; }
            RenderTargetIdentifier source { get; set; }
            CameraData cameraData { get; set; }
            RenderTextureDescriptor sourceDesc { get; set; }

            public void Setup(Shader shader, ScriptableRenderer renderer, RenderingData renderingData)
            {
                if (material == null) material = CoreUtils.CreateEngineMaterial(shader);

#if !URP_10_0_0_OR_NEWER
                source = renderer.cameraColorTarget;
                cameraData = renderingData.cameraData;

                FetchVolumeComponent();
                renderPassEvent = Output == SSCC.OutputEffectTo.Screen ? RenderPassEvent.BeforeRenderingTransparents : RenderPassEvent.BeforeRenderingOpaques;
#endif
            }

#if URP_10_0_0_OR_NEWER
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                source = renderingData.cameraData.renderer.cameraColorTarget;
                cameraData = renderingData.cameraData;

                FetchVolumeComponent();

                var passInput = ScriptableRenderPassInput.Depth;
                if (sscc.normalsSource.value == SSCC.PerPixelNormals.Camera)
                    passInput |= ScriptableRenderPassInput.Normal;

                ConfigureInput(passInput);

#if UNITY_2021_2_OR_NEWER
                ConfigureColorStoreAction(RenderBufferStoreAction.DontCare);
#endif
            
                renderPassEvent = Output == SSCC.OutputEffectTo.Screen ? RenderPassEvent.BeforeRenderingTransparents : RenderPassEvent.BeforeRenderingOpaques;

                if (Output == SSCC.OutputEffectTo._SSCCTexture)
                    ConfigureTarget(BuiltinRenderTextureType.None);
            }
#endif

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (material == null) return;

                FetchVolumeComponent();

                if (!sscc.IsActive()) return;

                cameraTextureDescriptor.msaaSamples = 1;
                cameraTextureDescriptor.depthBufferBits = 0;
                sourceDesc = cameraTextureDescriptor;

                CheckParameters();
                UpdateMaterialProperties();
                UpdateShaderKeywords();

                if (Output == SSCC.OutputEffectTo._SSCCTexture)
                    ConfigureTarget(BuiltinRenderTextureType.None);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                {
                    Debug.LogError("SSCC material has not been correctly initialized...");
                    return;
                }
                if (!sscc.IsActive()) return;
                var cmd = CommandBufferPool.Get("SSCC");

                if (Output == SSCC.OutputEffectTo._SSCCTexture)
                {
                    cmd.ReleaseTemporaryRT(ShaderProperties.globalSSCCTexture);
                    cmd.GetTemporaryRT(ShaderProperties.globalSSCCTexture, sourceDesc.width, sourceDesc.height, 0, FilterMode.Bilinear, GraphicsFormat.R16G16B16A16_SFloat/*.R32G32B32A32_SFloat*/);
                    cmd.SetGlobalTexture(ShaderProperties.globalSSCCTexture, new RenderTargetIdentifier(ShaderProperties.globalSSCCTexture, 0, CubemapFace.Unknown, -1));
                }
                else
                {
                    cmd.GetTemporaryRT(ShaderProperties.tempTex, sourceDesc, FilterMode.Point);
                    BlitFullscreenMesh(cmd, source, ShaderProperties.tempTex, material, Pass.Copy);
                }

                if (Output == SSCC.OutputEffectTo.Screen)
                    BlitFullscreenMesh(cmd, ShaderProperties.tempTex, source, material, Pass.Composite);
                else
                    BlitFullscreenMesh(cmd, ShaderProperties.globalSSCCTexture, material, Pass.Composite);

                if (Output == SSCC.OutputEffectTo.Screen)
                    cmd.ReleaseTemporaryRT(ShaderProperties.tempTex);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            public void Cleanup()
            {
                CoreUtils.Destroy(material);
            }

            private void FetchVolumeComponent()
            {
                if (sscc == null)
                    sscc = VolumeManager.instance.stack.GetComponent<SSCC>();
            }
            
            void UpdateMaterialProperties()
            {
                var sourceWidth = cameraData.cameraTargetDescriptor.width;
                var sourceHeight = cameraData.cameraTargetDescriptor.height;

                float tanHalfFovY = Mathf.Tan(0.5f * cameraData.camera.fieldOfView * Mathf.Deg2Rad);
                float invFocalLenX = 1.0f / (1.0f / tanHalfFovY * (sourceHeight / (float)sourceWidth));
                float invFocalLenY = 1.0f / (1.0f / tanHalfFovY);

                material.SetVector(ShaderProperties.inputTexelSize, new Vector4(1f / sourceWidth, 1f / sourceHeight, sourceWidth, sourceHeight));
                material.SetVector(ShaderProperties.uvToView, new Vector4(2.0f * invFocalLenX, -2.0f * invFocalLenY, -1.0f * invFocalLenX, 1.0f * invFocalLenY));
                material.SetMatrix(ShaderProperties.worldToCameraMatrix, cameraData.camera.worldToCameraMatrix);

                material.SetFloat(ShaderProperties.effectIntensity, sscc.effectIntensity.value);
                material.SetFloat(ShaderProperties.distanceFade, sscc.distanceFade.value);

                material.SetFloat(ShaderProperties.curvaturePixelRadius, new float[] { 0f, 0.5f, 1f, 1.5f, 2.5f }[sscc.curvaturePixelRadius.value]);
                material.SetFloat(ShaderProperties.curvatureRidge, sscc.curvatureBrights.value == 0f ? 999f : (5f - sscc.curvatureBrights.value));
                material.SetFloat(ShaderProperties.curvatureValley, sscc.curvatureDarks.value == 0f ? 999f : (5f - sscc.curvatureDarks.value));

                material.SetFloat(ShaderProperties.cavityWorldRadius, sscc.cavityRadius.value);
                material.SetFloat(ShaderProperties.cavityRidge, sscc.cavityBrights.value * 2f);
                material.SetFloat(ShaderProperties.cavityValley, sscc.cavityDarks.value * 2f);
            }

            void UpdateShaderKeywords()
            {
                material.shaderKeywords = new string[]
                {
                    cameraData.camera.orthographic ? "ORTHOGRAPHIC_PROJECTION" :  "__",
                    sscc.debugMode.value == SSCC.DebugMode.EffectOnly ? "DEBUG_EFFECT" : sscc.debugMode.value == SSCC.DebugMode.ViewNormals ? "DEBUG_NORMALS" : "__",
                    sscc.normalsSource.value == SSCC.PerPixelNormals.ReconstructedFromDepth ? "NORMALS_RECONSTRUCT" : "__",
                    sscc.cavitySamples.value == SSCC.CavitySamples.Low6 ? "CAVITY_SAMPLES_6" : sscc.cavitySamples.value == SSCC.CavitySamples.Medium8 ? "CAVITY_SAMPLES_8" : sscc.cavitySamples.value == SSCC.CavitySamples.High12 ? "CAVITY_SAMPLES_12" : "",
                    sscc.saturateCavity.value ? "SATURATE_CAVITY" : "__",
                    Output == SSCC.OutputEffectTo._SSCCTexture ? "OUTPUT_TO_TEXTURE" : "__"
                };
            }



            //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void BlitFullscreenMesh(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex = 0)
            {
                cmd.SetGlobalTexture(ShaderProperties.mainTex, source);
                cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

            public void BlitFullscreenMesh(CommandBuffer cmd, RenderTargetIdentifier destination, Material material, int passIndex = 0)
            {
                cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }

        }

        [SerializeField]
        [Space(15)]
        [Header("You can now add SSCC to your Post Process Volume.")]
        Shader shader;
        private SSCCRenderPass renderPass;

        public override void Create()
        {
            if (!isActive)
            {
                renderPass?.Cleanup();
                renderPass = null;
                return;
            }

            name = "SSCC";

            renderPass = new SSCCRenderPass();
        }

        void OnDisable()
        {
            renderPass?.Cleanup();
        }

#if URP_10_0_0_OR_NEWER
        protected override void Dispose(bool disposing)
        {
            renderPass?.Cleanup();
            renderPass = null;
        }
#endif

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            shader = Shader.Find("Hidden/Universal Render Pipeline/SSCC");
            if (shader == null)
            {
                Debug.LogWarning("SSCC shader was not found. Please ensure it compiles correctly");
                return;
            }

            if (renderingData.cameraData.postProcessEnabled)
            {
                renderPass.Setup(shader, renderer, renderingData);
                renderer.EnqueuePass(renderPass);
            }
        }
    }
}
