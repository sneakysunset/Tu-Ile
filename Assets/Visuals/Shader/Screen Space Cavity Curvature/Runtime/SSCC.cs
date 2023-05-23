using UnityEngine;
using UnityEngine.Rendering;
#if ENABLE_VR
using XRSettings = UnityEngine.XR.XRSettings;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;

namespace ScreenSpaceCavityCurvature
{
    [ExecuteInEditMode, ImageEffectAllowedInSceneView, AddComponentMenu("SSCC Screen Space Cavity Curvature")]
    [RequireComponent(typeof(Camera))]
    public class SSCC : MonoBehaviour
    {
        public enum PerPixelNormals { DeferredGBuffer, Camera, ReconstructedFromDepth }
        public enum DebugMode { Disabled, EffectOnly, ViewNormals }
        public enum CavitySamples { Low6, Medium8, High12 }
        public enum OutputEffectTo { Screen, [InspectorName("_SSCCTexture in shaders")] _SSCCTexture }

        [HideInInspector] public Shader ssccShader;

        //
        [Tooltip("Lerps the whole effect from 0 to 1.")] [Range(0f, 1f)] public float effectIntensity = 1f;
        [Tooltip("Divides effect intensity by (depth * distanceFade).\nZero means effect doesn't fade with distance.")] [Range(0f, 1f)] public float distanceFade = 0f;

        [Space(6)]

        [Tooltip("The radius of curvature calculations in pixels.")] [Range(0, 4)] public int curvaturePixelRadius = 2;
        [Tooltip("How bright does curvature get.")] [Range(0f, 5f)] public float curvatureBrights = 2f;
        [Tooltip("How dark does curvature get.")] [Range(0f, 5f)] public float curvatureDarks = 3f;

        [Space(6)]

        [Tooltip("The amount of samples used for cavity calculation.")] public CavitySamples cavitySamples = CavitySamples.Low6;
        [Tooltip("True: uses pow() blending to make colors more saturated in bright/dark areas of cavity.\n\nFalse: uses additive blending.")] public bool saturateCavity = true;
        [Tooltip("The radius of cavity calculations in world units.")] [Range(0f, 0.5f)] public float cavityRadius = 0.25f;
        [Tooltip("How bright does cavity get.")] [Range(0f, 5f)] public float cavityBrights = 3f;
        [Tooltip("How dark does cavity get.")] [Range(0f, 5f)] public float cavityDarks = 2f;

        [Space(6)]

        [Tooltip("Where to get normals from.")] public PerPixelNormals normalsSource = PerPixelNormals.Camera;
        [Tooltip("May be useful to check what objects contribute normals, as objects that do not contribute their normals will not contribute to the effect.")] public DebugMode debugMode = DebugMode.Disabled;

        [Space(6)]

        [Tooltip("Screen: Applies the effect over the entire screen.\n\n_SSCCTexture: Instead of writing the effect to the screen, will write the effect into a global shader texture named _SSCCTexture, so you can sample it selectively in your shaders and exclude certain objects from receiving outlines etc. See \"Output To Texture Examples\" folder for example shaders.")] public OutputEffectTo output = OutputEffectTo.Screen;
        //

        void CheckParameters()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                Debug.LogWarning("Please follow the SRP usage instructions.");
                enabled = false;
            }

            ssccCamera.depthTextureMode |= DepthTextureMode.Depth;
            if (normalsSource == PerPixelNormals.Camera) ssccCamera.depthTextureMode |= DepthTextureMode.DepthNormals;

            if (ssccCamera.actualRenderingPath != RenderingPath.DeferredShading && normalsSource == PerPixelNormals.DeferredGBuffer) normalsSource = PerPixelNormals.Camera;

            if (stereoActive && ssccCamera.actualRenderingPath != RenderingPath.DeferredShading && normalsSource != PerPixelNormals.ReconstructedFromDepth) normalsSource = PerPixelNormals.ReconstructedFromDepth;
            
            if (output == OutputEffectTo._SSCCTexture && ssccCamera.actualRenderingPath == RenderingPath.DeferredShading) normalsSource = PerPixelNormals.ReconstructedFromDepth; //cant get correct normals texture in deferred
        }

        OutputEffectTo Output => debugMode != DebugMode.Disabled ? OutputEffectTo.Screen : output;

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

        Material material;
        Camera ssccCamera;
        CommandBuffer cmdBuffer;
        int width;
        int height;
        bool stereoActive;
        XRSettings.StereoRenderingMode stereoRenderingMode;
        int screenWidth;
        int screenHeight;

        CameraEvent cameraEvent => Output == OutputEffectTo.Screen ? CameraEvent.BeforeImageEffectsOpaque : CameraEvent.BeforeForwardOpaque;
        CameraEvent[] possibleCameraEvents = new[] { CameraEvent.BeforeImageEffectsOpaque, CameraEvent.BeforeForwardOpaque };

        Mesh fullscreenTriangle
        {
            get
            {
                if (m_FullscreenTriangle != null) return m_FullscreenTriangle;
                m_FullscreenTriangle = new Mesh { name = "Fullscreen Triangle" };
                m_FullscreenTriangle.SetVertices(new List<Vector3> { new Vector3(-1f, -1f, 0f), new Vector3(-1f, 3f, 0f), new Vector3(3f, -1f, 0f) });
                m_FullscreenTriangle.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
                m_FullscreenTriangle.UploadMeshData(false);
                return m_FullscreenTriangle;
            }
        }

        bool isCommandBufferDirty
        {
            get
            {
                if (m_IsCommandBufferDirty || m_PreviousDebugMode != debugMode || m_PreviousWidth != width || m_PreviousHeight != height || m_PreviousRenderingPath != ssccCamera.actualRenderingPath
                    || m_PreviousOutputEffectTo != Output || m_PreviousCameraEvent != cameraEvent)
                {
                    m_PreviousDebugMode = debugMode; m_PreviousWidth = width; m_PreviousHeight = height; m_PreviousRenderingPath = ssccCamera.actualRenderingPath;
                    m_PreviousOutputEffectTo = Output; m_PreviousCameraEvent = cameraEvent;
                    return true;
                }
                return false;
            }
            set
            {
                m_IsCommandBufferDirty = value;
            }
        }

        RenderTextureDescriptor m_sourceDescriptor;
        bool m_IsCommandBufferDirty;
        Mesh m_FullscreenTriangle;
        DebugMode? m_PreviousDebugMode;
        int m_PreviousWidth;
        int m_PreviousHeight;
        RenderingPath m_PreviousRenderingPath;
        OutputEffectTo m_PreviousOutputEffectTo;
        CameraEvent m_PreviousCameraEvent;

        static RenderTextureFormat defaultHDRRenderTextureFormat
        {
            get
            {
                #if UNITY_ANDROID || UNITY_IPHONE || UNITY_TVOS || UNITY_SWITCH || UNITY_EDITOR
                    RenderTextureFormat format = RenderTextureFormat.RGB111110Float;
                    #if UNITY_EDITOR
                        var target = EditorUserBuildSettings.activeBuildTarget;
                        if (target != BuildTarget.Android && target != BuildTarget.iOS && target != BuildTarget.tvOS && target != BuildTarget.Switch)
                            return RenderTextureFormat.DefaultHDR;
                    #endif
                    if (SystemInfo.SupportsRenderTextureFormat(format))
                        return format;
                #endif
                return RenderTextureFormat.DefaultHDR;
            }
        }
        RenderTextureFormat sourceFormat { get { return ssccCamera.allowHDR ? defaultHDRRenderTextureFormat : RenderTextureFormat.Default; } }
        static RenderTextureFormat colorFormat { get { return SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default; } }

        void OnEnable()
        {
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                Debug.LogWarning("SSCC shader is not supported on this platform.");
                this.enabled = false;
                return;
            }

            if (ssccShader == null) ssccShader = Shader.Find("Hidden/SSCC");
            if (ssccShader == null)
            {
                Debug.LogError("SSCC shader was not found...");
                return;
            }

            if (!ssccShader.isSupported)
            {
                Debug.LogWarning("SSCC shader is not supported on this platform.");
                this.enabled = false;
                return;
            }

            Initialize();
        }

        void OnDisable()
        {
            ClearCommandBuffer(cmdBuffer);

            if (material != null)
                DestroyImmediate(material);
            if (fullscreenTriangle != null)
                DestroyImmediate(fullscreenTriangle);
        }

        void OnPreRender()
        {
            if (ssccShader == null || ssccCamera == null) return;

            FetchRenderParameters();
            CheckParameters();
            UpdateMaterialProperties();
            UpdateShaderKeywords();

            if (isCommandBufferDirty)
            {
                ClearCommandBuffer(cmdBuffer);
                BuildCommandBuffer(cmdBuffer);
                ssccCamera.AddCommandBuffer(cameraEvent, cmdBuffer);
                isCommandBufferDirty = false;
            }
        }

        void OnValidate()
        {
            if (ssccShader == null || ssccCamera == null) return;

            CheckParameters();
        }

        void Initialize()
        {
            m_sourceDescriptor = new RenderTextureDescriptor(0, 0);

            ssccCamera = GetComponent<Camera>();
            ssccCamera.forceIntoRenderTexture = true;

            material = new Material(ssccShader);
            material.hideFlags = HideFlags.HideAndDontSave;

            cmdBuffer = new CommandBuffer { name = "SSCC" };
            isCommandBufferDirty = true;
        }

        void FetchRenderParameters()
        {
            #if !UNITY_SWITCH && ENABLE_VR
            if (ssccCamera.stereoEnabled)
            {
                var xrDesc = XRSettings.eyeTextureDesc;
                stereoRenderingMode = XRSettings.StereoRenderingMode.SinglePass;

                if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass)
                    stereoRenderingMode = XRSettings.StereoRenderingMode.MultiPass;

                #if UNITY_STANDALONE || UNITY_EDITOR || UNITY_PS4
                if (xrDesc.dimension == TextureDimension.Tex2DArray)
                    stereoRenderingMode = XRSettings.StereoRenderingMode.SinglePassInstanced;
                #endif

                if (stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass)
                {
                    //xrDesc.width /= 2;
                    xrDesc.vrUsage = VRTextureUsage.None;
                }

                width = xrDesc.width;
                height = xrDesc.height;
                m_sourceDescriptor = xrDesc;

                screenWidth = XRSettings.eyeTextureWidth;
                screenHeight = XRSettings.eyeTextureHeight;

                if (stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass)
                    screenWidth /= 2;

                stereoActive = true;
            }
            else
            #endif
            {
                width = ssccCamera.pixelWidth;
                height = ssccCamera.pixelHeight;
                m_sourceDescriptor.width = width;
                m_sourceDescriptor.height = height;
                screenWidth = width;
                screenHeight = height;
                stereoActive = false;
            }

        }

        void ClearCommandBuffer(CommandBuffer cmd)
        {
            if (cmd != null)
            {
                //if (ssccCamera != null) ssccCamera.RemoveCommandBuffer(cameraEvent, cmd);
                if (ssccCamera != null) foreach (var camEvent in possibleCameraEvents) ssccCamera.RemoveCommandBuffer(camEvent, cmd);
                cmd.Clear();
            }
        }

        void BuildCommandBuffer(CommandBuffer cmd)
        {
            if (Output == OutputEffectTo._SSCCTexture)
            {
                cmd.ReleaseTemporaryRT(ShaderProperties.globalSSCCTexture);
                //GetScreenSpaceTemporaryRT(cmd, ShaderProperties.globalSSCCTexture, colorFormat: sourceFormat);
                cmd.GetTemporaryRT(ShaderProperties.globalSSCCTexture, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
                cmd.SetGlobalTexture(ShaderProperties.globalSSCCTexture, new RenderTargetIdentifier(ShaderProperties.globalSSCCTexture, 0, CubemapFace.Unknown, -1));
            }
            else
            {
                cmd.SetGlobalTexture(ShaderProperties.globalSSCCTexture, BuiltinRenderTextureType.None);
                GetScreenSpaceTemporaryRT(cmd, ShaderProperties.tempTex, colorFormat: sourceFormat);
                if (stereoActive && ssccCamera.actualRenderingPath != RenderingPath.DeferredShading)
                    cmd.Blit(BuiltinRenderTextureType.CameraTarget, ShaderProperties.tempTex);
                else
                    BlitFullscreenTriangle(cmd, BuiltinRenderTextureType.CameraTarget, ShaderProperties.tempTex, material, Pass.Copy);
            }

            cmd.SetGlobalVector(ShaderProperties.uvTransform, SystemInfo.graphicsUVStartsAtTop ? new Vector4(1f, -1f, 0f, 1f) : new Vector4(1f, 1f, 0f, 0f));

            if (Output == OutputEffectTo.Screen)
                BlitFullscreenTriangle(cmd, ShaderProperties.tempTex, BuiltinRenderTextureType.CameraTarget, material, Pass.Composite);
            else
                BlitFullscreenTriangle(cmd, ShaderProperties.globalSSCCTexture, material, Pass.Composite);

            if (Output == OutputEffectTo.Screen)
                cmd.ReleaseTemporaryRT(ShaderProperties.tempTex);
        }

        void UpdateMaterialProperties()
        {
            float tanHalfFovY = Mathf.Tan(0.5f * ssccCamera.fieldOfView * Mathf.Deg2Rad);
            float invFocalLenX = 1.0f / (1.0f / tanHalfFovY * (screenHeight / (float)screenWidth));
            float invFocalLenY = 1.0f / (1.0f / tanHalfFovY);
            
            material.SetVector(ShaderProperties.inputTexelSize, new Vector4(1f / width, 1f / height, width, height));
            material.SetVector(ShaderProperties.uvToView, new Vector4(2.0f * invFocalLenX, -2.0f * invFocalLenY, -1.0f * invFocalLenX, 1.0f * invFocalLenY));
            material.SetMatrix(ShaderProperties.worldToCameraMatrix, ssccCamera.worldToCameraMatrix);

            material.SetFloat(ShaderProperties.effectIntensity, effectIntensity);
            material.SetFloat(ShaderProperties.distanceFade, distanceFade);

            material.SetFloat(ShaderProperties.curvaturePixelRadius, new float[] { 0f, 0.5f, 1f, 1.5f, 2.5f }[curvaturePixelRadius]);
            material.SetFloat(ShaderProperties.curvatureRidge, curvatureBrights == 0f ? 999f : (5f - curvatureBrights));
            material.SetFloat(ShaderProperties.curvatureValley, curvatureDarks == 0f ? 999f : (5f - curvatureDarks));

            material.SetFloat(ShaderProperties.cavityWorldRadius, cavityRadius);
            material.SetFloat(ShaderProperties.cavityRidge, cavityBrights * 2f);
            material.SetFloat(ShaderProperties.cavityValley, cavityDarks * 2f);
        }

        void UpdateShaderKeywords()
        {
            material.shaderKeywords = new string[]
            {
                ssccCamera.orthographic ? "ORTHOGRAPHIC_PROJECTION" :  "__",
                debugMode == DebugMode.EffectOnly ? "DEBUG_EFFECT" : debugMode == DebugMode.ViewNormals ? "DEBUG_NORMALS" : "__",
                normalsSource == PerPixelNormals.Camera ? "NORMALS_CAMERA" : normalsSource == PerPixelNormals.ReconstructedFromDepth ? "NORMALS_RECONSTRUCT" : "__",
                cavitySamples == CavitySamples.Low6 ? "CAVITY_SAMPLES_6" : cavitySamples == CavitySamples.Medium8 ? "CAVITY_SAMPLES_8" : cavitySamples == CavitySamples.High12 ? "CAVITY_SAMPLES_12" : "",
                saturateCavity ? "SATURATE_CAVITY" : "__",
                Output == OutputEffectTo._SSCCTexture ? "OUTPUT_TO_TEXTURE" : "__"
            };
        }



        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        RenderTextureDescriptor GetDefaultDescriptor(int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default)
        {
            var modifiedDesc = new RenderTextureDescriptor(m_sourceDescriptor.width, m_sourceDescriptor.height, m_sourceDescriptor.colorFormat, depthBufferBits);
            modifiedDesc.dimension = m_sourceDescriptor.dimension;
            modifiedDesc.volumeDepth = m_sourceDescriptor.volumeDepth;
            modifiedDesc.vrUsage = m_sourceDescriptor.vrUsage;
            modifiedDesc.msaaSamples = m_sourceDescriptor.msaaSamples;
            modifiedDesc.memoryless = m_sourceDescriptor.memoryless;

            modifiedDesc.useMipMap = m_sourceDescriptor.useMipMap;
            modifiedDesc.autoGenerateMips = m_sourceDescriptor.autoGenerateMips;
            modifiedDesc.enableRandomWrite = m_sourceDescriptor.enableRandomWrite;
            modifiedDesc.shadowSamplingMode = m_sourceDescriptor.shadowSamplingMode;

            if (ssccCamera.allowDynamicResolution)
                modifiedDesc.useDynamicScale = true; //IF YOU ARE GETTING AN ERROR HERE, UNFORTUNATELY YOUR UNITY VERSION IS TOO LOW FOR THIS ASSET

            if (colorFormat != RenderTextureFormat.Default)
                modifiedDesc.colorFormat = colorFormat;

            if (readWrite == RenderTextureReadWrite.sRGB)
                modifiedDesc.sRGB = true;
            else if (readWrite == RenderTextureReadWrite.Linear)
                modifiedDesc.sRGB = false;
            else if (readWrite == RenderTextureReadWrite.Default)
                modifiedDesc.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;

            return modifiedDesc;
        }

        RenderTexture GetScreenSpaceRT(int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
                                               FilterMode filter = FilterMode.Bilinear, int widthOverride = 0, int heightOverride = 0)
        {
            var desc = GetDefaultDescriptor(depthBufferBits, colorFormat, readWrite);
            if (widthOverride > 0)
                desc.width = widthOverride;
            if (heightOverride > 0)
                desc.height = heightOverride;

            //intermediates in VR are unchanged
            if (stereoActive && desc.dimension == TextureDimension.Tex2DArray)
                desc.dimension = TextureDimension.Tex2D;

            var rt = new RenderTexture(desc);
            rt.filterMode = filter;
            return rt;
        }

        void GetScreenSpaceTemporaryRT(CommandBuffer cmd, int nameID,
                                               int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
                                               FilterMode filter = FilterMode.Bilinear, int widthOverride = 0, int heightOverride = 0)
        {
            var desc = GetDefaultDescriptor(depthBufferBits, colorFormat, readWrite);
            if (widthOverride > 0)
                desc.width = widthOverride;
            if (heightOverride > 0)
                desc.height = heightOverride;

            //intermediates in VR are unchanged
            if (stereoActive && desc.dimension == TextureDimension.Tex2DArray)
                desc.dimension = TextureDimension.Tex2D;

            cmd.GetTemporaryRT(nameID, desc, filter);
        }

        void BlitFullscreenTriangle(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int pass = 0)
        {
            cmd.SetGlobalTexture(ShaderProperties.mainTex, source);
            cmd.SetRenderTarget(destination);
            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
        }

        void BlitFullscreenTriangle(CommandBuffer cmd, RenderTargetIdentifier destination, Material material, int pass = 0)
        {
            cmd.SetRenderTarget(destination);
            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
        }

        void BlitFullscreenTriangleWithClear(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, Color clearColor, int pass = 0)
        {
            cmd.SetGlobalTexture(ShaderProperties.mainTex, source);
            cmd.SetRenderTarget(destination);
            cmd.ClearRenderTarget(false, true, clearColor);
            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
        }
        
    }
}
