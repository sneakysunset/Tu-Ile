using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace ProjectDawn.SplitScreen
{
    [System.Serializable]
    public class ScreenData
    {
        public Camera Camera;
        public Transform Target;
        [NonSerialized]
        public RenderTexture RenderTarget;
        [NonSerialized]
        public Mesh Mesh;
    }

    [System.Flags]
    public enum DrawFlags
    {
        None,
        Regions = 1 << 1,
        DelaunayDual = 1 << 4,
    }

    public enum BalancingMode
    {
        None,
        UniformTransformRelaxation,
    }

    [System.Serializable]
    public struct Balancing
    {
        public bool Enabled;
        public float StepSize;
        [Range(0, 5)]
        public int RelaxationIterations;

        public bool Active => Enabled;

        public static Balancing Default => new Balancing
        {
            RelaxationIterations = 1,
            StepSize = 0.009f,
        };
    }

    public enum CameraCentering
    {
        None,
        ScreenCentered,
        PlayerCentered,
    }

    public enum BlendShape
    {
        Region,
        Circle,
    }

    [System.Serializable]
    public struct Translating
    {
        [MaxValue(0)]
        public float Blend;
        public BlendShape BlendShape;
        public CameraCentering Centering;
        [HideInInspector]
        [Range(0, 0.5f)]
        public float Border;

        public static Translating Default => new Translating
        {
            BlendShape = BlendShape.Region,
            Blend = 0.2f,
            Centering = CameraCentering.ScreenCentered,
        };
    }

    /// <summary>
    /// Split screen implementation with the <see cref="MonoBehaviour"/> component.
    /// This component adaptively constructs splits and combines them for camera to present.
    /// It only works with up to 4 split screens.
    /// </summary>
    [AddComponentMenu("Rendering/Split Screen Effect")]
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class SplitScreenEffect : MonoBehaviour, ISerializationCallbackReceiver
    {
        static bool MinimizeCameraRect = false;

        static class Markers
        {
            public static readonly ProfilerMarker OnBeforePositionProjecting = new ProfilerMarker("OnBeforePositionProjecting");
            public static readonly ProfilerMarker Prepare = new ProfilerMarker("Prepare");
            public static readonly ProfilerMarker CreateScreens = new ProfilerMarker("CreateScreens");
            public static readonly ProfilerMarker BalanceScreens = new ProfilerMarker("BalanceScreens");
            public static readonly ProfilerMarker UpdateScreens = new ProfilerMarker("UpdateScreens");
            public static readonly ProfilerMarker UpdateMesh = new ProfilerMarker("UpdateMesh");
            public static readonly ProfilerMarker UpdateCamera = new ProfilerMarker("UpdateCamera");
            public static readonly ProfilerMarker UpdateCommandBuffer = new ProfilerMarker("UpdateCommandBuffer");
        }

        [Reload("Packages/com.projectdawn.splitscreen/ProjectDawn.SplitScreen/Default-SplitScreen.mat")]
        public Material Material;
        public List<ScreenData> Screens = new List<ScreenData>();
        [MaxValue(0)]
        public float Distance = 10;
        [Range(0, 2)]
        public float BoundsRadius = 0.3f;
        public Translating Translating = Translating.Default;
        public Balancing Balancing = Balancing.Default;
        public bool UseAspectRatio = true;
        public DrawFlags DrawFlags = DrawFlags.Regions;

        [SerializeField]
        int m_Version = 0;

        CommandBuffer m_Cmd;
        List<MonoBehaviour> m_Components = new List<MonoBehaviour>();
        SplitScreen2 m_SplitScreen2;
        SplitScreen3 m_SplitScreen3;
        SplitScreen4 m_SplitScreen4;
        ScreenRegions m_ScreenRegions;
        bool m_CommandBufferModified;

        float4x4 m_WorldToPlane;
        float4x4 m_PlaneToWorld;
        float m_AspectRation;
        float m_Scale;

        public bool IsCreated { private set; get; }
        public bool IsValid => Screens != null && Screens.Count != 0 && Material != null;

        /// <summary>
        /// Returns command buffer that contains commands for rendering current frame split screen.
        /// </summary>
        public CommandBuffer GetCommandBuffer()
        {
            CheckIsCreated();
            return m_Cmd;
        }

        /// <summary>
        /// Add new screen.
        /// </summary>
        public void AddScreen(Camera camera, Transform target)
        {
            CheckIsCreated();
            Screens.Add(new ScreenData { Camera = camera, Target = target });
        }

        /// <summary>
        /// Clears all screen data.
        /// </summary>
        public void Clear()
        {
            CheckIsCreated();
            Screens.Clear();
        }

        /// <summary>
        /// Returns the value of how much the screens are balanced. Where 0 means fully balanced.
        /// </summary>
        public float GetBalanceError()
        {
            switch (Screens.Count)
            {
                case 3:
                    if (m_SplitScreen3.IsCreated)
                        return m_SplitScreen3.GetBalanceError();
                    return float.NaN;
                case 4:
                    if (m_SplitScreen4.IsCreated)
                        return m_SplitScreen4.GetBalanceError();
                    return float.NaN;
                default:
                    return float.NaN;
            }
        }

        /// <summary>
        /// Adds to the command buffer commands needed to render split screen.
        /// </summary>
        public void UpdateCommandBuffer(CommandBuffer cmd)
        {
            if (!m_ScreenRegions.IsCreated)
                return;

            using (Markers.UpdateCommandBuffer.Auto())
            {
                cmd.Clear();
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                for (int i = 0; i < Screens.Count; ++i)
                {
                    cmd.SetGlobalTexture("_SplitScreenTexture", Screens[i].RenderTarget);
                    cmd.DrawMesh(Screens[i].Mesh, Matrix4x4.identity, Material, 0, 0);
                }

                foreach (var component in m_Components)
                {
                    if (component is ISplitScreenCommandBuffer hook && component.isActiveAndEnabled)
                    {
                        hook.OnSplitScreenCommandBuffer(cmd, in m_ScreenRegions);
                    }
                }
            }
        }

        void LateUpdate()
        {
            if (!IsValid)
                return;

            var camera = GetComponent<Camera>();

            using (Markers.Prepare.Auto())
            {
                UpdateBounds();
                UpdatePlaneMatrices();
                GetComponents<MonoBehaviour>(m_Components);
            }

            var screenRegions = m_ScreenRegions;
            switch (Screens.Count)
            {
                case 1:
                    // TODO: use directly main camera for rendering

                    screenRegions.Clear();

                    screenRegions.Regions.Add(new ScreenRegion
                    {
                        Center = 0,
                        Uv = 0,
                        Position = GetScreenTargetPositionPS(0),
                        Range = new RangeInt(0, 4),
                    });

                    // Construct viewport
                    screenRegions.Points.Add(new float2(-1, -1));
                    screenRegions.Points.Add(new float2(1, -1));
                    screenRegions.Points.Add(new float2(1, 1));
                    screenRegions.Points.Add(new float2(-1, 1));

                    break;

                case 2:
                    if (!m_SplitScreen2.IsCreated)
                        m_SplitScreen2 = new SplitScreen2(Allocator.Persistent);

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen2.Reset(
                            GetScreenTargetPositionPS(0),
                            GetScreenTargetPositionPS(1),
                            m_Scale,
                            m_AspectRation,
                            BoundsRadius
                        );
                    }

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen2.UpdatePositions(
                            GetScreenTranslatingPositionPS(0),
                            GetScreenTranslatingPositionPS(1)
                        );
                        m_SplitScreen2.CreateScreens(Translating, ref screenRegions);
                    }
                    break;

                case 3:
                    if (!m_SplitScreen3.IsCreated)
                        m_SplitScreen3 = new SplitScreen3(Allocator.Persistent);

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen3.Reset(
                            GetScreenTargetPositionPS(0),
                            GetScreenTargetPositionPS(1),
                            GetScreenTargetPositionPS(2),
                            m_Scale,
                            m_AspectRation,
                            BoundsRadius
                        );
                    }

                    if (Balancing.Active)
                    {
                        using (Markers.BalanceScreens.Auto())
                        {
                            bool balanceScreens = true;
                            foreach (var component in m_Components)
                            {
                                if (component is ISplitScreenBalancing modifier && component.isActiveAndEnabled)
                                {
                                    modifier.OnSplitScreenBalancing(m_SplitScreen3.VoronoiBuilder, m_SplitScreen3.VoronoiDiagram, m_SplitScreen3.Sites, Balancing);
                                    balanceScreens = false;
                                }
                            }

                            if (balanceScreens)
                            {
                                var balanceJob = new SplitScreen3BalanceJob
                                {
                                    SplitScreen = m_SplitScreen3,
                                    Balancing = Balancing,
                                };
                                balanceJob.Schedule().Complete();
                            }
                        }
                    }

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen3.UpdatePositions(
                            GetScreenTranslatingPositionPS(0),
                            GetScreenTranslatingPositionPS(1),
                            GetScreenTranslatingPositionPS(2)
                        );
                        m_SplitScreen3.CreateScreens(Translating, ref screenRegions);
                    }
                    break;

                case 4:
                    if (!m_SplitScreen4.IsCreated)
                        m_SplitScreen4 = new SplitScreen4(Allocator.Persistent);

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen4.Reset(
                            GetScreenTargetPositionPS(0),
                            GetScreenTargetPositionPS(1),
                            GetScreenTargetPositionPS(2),
                            GetScreenTargetPositionPS(3),
                            m_Scale,
                            m_AspectRation,
                            BoundsRadius);
                    }

                    if (Balancing.Active)
                    {
                        using (Markers.BalanceScreens.Auto())
                        {
                            bool balanceScreens = true;
                            foreach (var component in m_Components)
                            {
                                if (component is ISplitScreenBalancing modifier && component.isActiveAndEnabled)
                                {
                                    modifier.OnSplitScreenBalancing(m_SplitScreen4.VoronoiBuilder, m_SplitScreen4.VoronoiDiagram, m_SplitScreen4.Sites, Balancing);
                                    balanceScreens = false;
                                }
                            }

                            if (balanceScreens)
                            {
                                var balanceJob = new SplitScreen4BalanceJob
                                {
                                    SplitScreen = m_SplitScreen4,
                                    Balancing = Balancing,
                                };
                                balanceJob.Schedule().Complete();
                            }
                        }
                    }

                    using (Markers.CreateScreens.Auto())
                    {
                        m_SplitScreen4.UpdatePositions(
                            GetScreenTranslatingPositionPS(0),
                            GetScreenTranslatingPositionPS(1),
                            GetScreenTranslatingPositionPS(2),
                            GetScreenTranslatingPositionPS(3)
                        );

                        m_SplitScreen4.CreateScreens(Translating, ref screenRegions);
                    }
                break;

                default:
                    throw new NotSupportedException("Split screen only supports up to 4.");
            }

            bool updateCommandBuffer = false;
            using (Markers.UpdateScreens.Auto())
            {
                for (int i = 0; i < screenRegions.Length; ++i)
                {
                    updateCommandBuffer |= UpdateScreen(i, screenRegions.GetRegion(i), screenRegions.GetRegionPoints(i), screenRegions.GetRegionRect(i));
                }

                // Update command buffer if there any modifiers that will change it
                foreach (var component in m_Components)
                {
                    if (component is ISplitScreenCommandBuffer hook && component.isActiveAndEnabled)
                    {
                        updateCommandBuffer = true;
                        m_CommandBufferModified = true;
                    }
                }

                // Update command buffer if all modifiers are gone, but buffer is still left with previous modifications
                if (!updateCommandBuffer && m_CommandBufferModified)
                {
                    updateCommandBuffer = true;
                    m_CommandBufferModified = false;
                }
            }

            if (updateCommandBuffer)
            {
                UpdateCommandBuffer(m_Cmd);
            }
        }

        void OnEnable()
        {
            m_Cmd = new CommandBuffer();
            m_Cmd.name = "Split Screen";

            // Builtin render pipeline
            var camera = GetComponent<Camera>();
            camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_Cmd);

            m_ScreenRegions = new ScreenRegions(Allocator.Persistent);

            IsCreated = true;
        }

        void OnDisable()
        {
            foreach (var screen in Screens)
            {
                screen.Mesh = null;
                screen.RenderTarget?.Release();
                screen.RenderTarget = null;
            }

            if (m_SplitScreen2.IsCreated)
                m_SplitScreen2.Dispose();
            if (m_SplitScreen3.IsCreated)
                m_SplitScreen3.Dispose();
            if (m_SplitScreen4.IsCreated)
                m_SplitScreen4.Dispose();

            m_ScreenRegions.Dispose();

            // Builtin render pipeline
            var camera = GetComponent<Camera>();
            camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_Cmd);

            IsCreated = false;
        }

        void OnDrawGizmos()
        {
            if (!IsValid)
                return;

            ISplitScreen splitScreen;
            switch (Screens.Count)
            {
                case 2:
                    splitScreen = m_SplitScreen2;
                    break;
                case 3:
                    splitScreen = m_SplitScreen3;
                    break;
                case 4:
                    splitScreen = m_SplitScreen4;
                    break;
                default:
                    return;
            }

            if (splitScreen.IsCreated && (DrawFlags & SplitScreen.DrawFlags.Regions) != 0)
            {
                splitScreen.DrawRegions(Translating.BlendShape);
            }

            if (splitScreen.IsCreated && (DrawFlags & SplitScreen.DrawFlags.DelaunayDual) != 0)
            {
                splitScreen.DrawDelaunayDual();
            }
        }

        bool UpdateScreen(int screenIndex, ScreenRegion region, NativeSlice<float2> points, Rect rect)
        {
            if (points.Length < 3)
                throw new ArgumentException($"Something failed and screen region at index {screenIndex} was incorrectly generated.");

            var camera = GetComponent<Camera>();
            var screen = Screens[screenIndex];
            var screenCamera = screen.Camera;
            bool dirty = false;

            float width = Screen.width;
            float height = Screen.height;

            // Updates render texture
            if (screen.RenderTarget == null || (screen.RenderTarget.width != width || screen.RenderTarget.height != height))
            {
                screen.RenderTarget?.Release();
                screen.RenderTarget = new RenderTexture((int)width, (int)height, 24);
                //screen.RenderTarget.filterMode = FilterMode.Point;
                screen.RenderTarget.name = $"ScreenTexture{screenIndex}";
                dirty = true;
            }

            // Updates camera
            if (screenCamera != null)
            {
                using (Markers.UpdateCamera.Auto())
                {
                    // Update position
                    var position = region.Position;
                    Matrix4x4 viewMatrix = m_PlaneToWorld;
                    var cameraPosition = viewMatrix.MultiplyPoint(new Vector3(position.x, position.y, position.z)) + transform.rotation * new Vector3(0, 0, -Distance);

                    screenCamera.transform.position = cameraPosition;

                    screenCamera.transform.rotation = camera.transform.rotation;

                    // Keep in sync other camera options
                    screenCamera.orthographicSize = camera.orthographicSize;
                    screenCamera.orthographic = camera.orthographic;
                    screenCamera.fieldOfView = camera.fieldOfView;
                    screenCamera.farClipPlane = camera.farClipPlane;
                    screenCamera.nearClipPlane = camera.nearClipPlane;
                    screenCamera.fieldOfView = camera.fieldOfView;

                    screenCamera.targetTexture = screen.RenderTarget;

                    float2 rectMin = math.max(rect.min, 0);
                    float2 rectMax = math.min(rect.max, 1);
                    rect = new Rect(rectMin, rectMax - rectMin);

                    // Simulate scissor rect with viewport and projection matric offset
                    if (MinimizeCameraRect)
                    {
                        float inverseWidth = 1 / rect.width;
                        float inverseHeight = 1 / rect.height;
                        Matrix4x4 matrix1 = Matrix4x4.TRS(
                            new Vector3(-rect.x * 2 * inverseWidth, -rect.y * 2 * inverseHeight, 0),
                            Quaternion.identity, Vector3.one);
                        Matrix4x4 matrix2 = Matrix4x4.TRS(
                            new Vector3(inverseWidth - 1, inverseHeight - 1, 0),
                            Quaternion.identity,
                            new Vector3(inverseWidth, inverseHeight, 1) );
                        screenCamera.projectionMatrix = matrix1 * matrix2 * camera.projectionMatrix;

                        screenCamera.rect = rect;
                    }
                    else
                    {
                        screenCamera.projectionMatrix = camera.projectionMatrix;
                        screenCamera.rect = new Rect(0, 0, 1, 1);
                    }
                }
            }

            // Updates mesh
            using (Markers.UpdateMesh.Auto())
            {
                if (screen.Mesh == null)
                {
                    screen.Mesh = new Mesh();
                    screen.Mesh.name = $"ScreenMesh{screenIndex}";
                    dirty = true;
                }
                else
                {
                    screen.Mesh.Clear();
                }

                var mesh = screen.Mesh;

                var vertices = new NativeList<float3>(region.Range.length * 3, Allocator.Temp);
                var uvs = new NativeList<float2>(region.Range.length * 3, Allocator.Temp);
                var indices = new NativeList<int>(region.Range.length * 3, Allocator.Temp);

                ConvexPolygon.Triangulate(points, region.Center, region.Uv, ref vertices, ref uvs, ref indices);

                mesh.SetVertices(vertices.AsArray());
                mesh.SetUVs(0, uvs.AsArray());
                mesh.SetIndices(indices.AsArray(), MeshTopology.Triangles, 0);

                vertices.Dispose();
                uvs.Dispose();
                indices.Dispose();
            }

            return dirty;
        }

        void UpdatePlaneMatrices()
        {
            // TODO: Cleanup this method

            var camera = GetComponent<Camera>();

            Quaternion rotation = !camera.orthographic ? (Quaternion)quaternion.RotateX(math.radians(90)) : transform.rotation; // transform.rotation
            m_PlaneToWorld = float4x4.TRS(rotation * new Vector3(0, 0, -Distance), rotation, 1);

            m_PlaneToWorld.c2 = -m_PlaneToWorld.c2;
            m_WorldToPlane = math.inverse(m_PlaneToWorld);

            if (!UseAspectRatio)
            {
                if (!camera.orthographic)
                {
                    float4 lookAtSS = math.mul(camera.projectionMatrix, new float4(0, 0, -Distance, 1));
                    float z = lookAtSS.z / lookAtSS.w;

                    float4 minSS = new float4(-1, -1, z, 1);
                    float4 maxSS = new float4(1, 1, z, 1);

                    float4x4 invProjectionMatrix = math.inverse(camera.projectionMatrix);
                    float4 minPS = math.mul(invProjectionMatrix, minSS);
                    float4 maxPS = math.mul(invProjectionMatrix, maxSS);

                    minPS /= minPS.w;
                    maxPS /= maxPS.w;

                    float4x4 orth = float4x4.OrthoOffCenter(minPS.x, maxPS.x, minPS.y, maxPS.y, camera.nearClipPlane, camera.farClipPlane);
                    m_WorldToPlane = math.mul(orth, m_WorldToPlane);
                }
                else
                {
                    m_WorldToPlane = math.mul(camera.projectionMatrix, m_WorldToPlane);
                }
            }

            m_PlaneToWorld = math.inverse(m_WorldToPlane);
        }

        void UpdateBounds()
        {
            // TODO: Cleanup this method

            if (!UseAspectRatio)
            {
                m_AspectRation = 1;
                m_Scale = 2;
            }
            else
            {
                var camera = GetComponent<Camera>();

                float4 lookAtSS = math.mul(camera.projectionMatrix, new float4(0, 0, -Distance, 1));
                float z = lookAtSS.z / lookAtSS.w;

                float4 minSS = new float4(-1, -1, z, 1);
                float4 maxSS = new float4(1, 1, z, 1);

                float4x4 invProjectionMatrix = math.inverse(camera.projectionMatrix);
                float4 minPS = math.mul(invProjectionMatrix, minSS);
                float4 maxPS = math.mul(invProjectionMatrix, maxSS);

                minPS /= minPS.w;
                maxPS /= maxPS.w;

                float2 size = maxPS.xy - minPS.xy;
                m_AspectRation = size.x / size.y;
                m_Scale = size.x;
            }
        }

        float3 GetScreenTargetPositionPS(int screenIndex)
        {
            var target = Screens[screenIndex].Target;

            var positionWS = target != null ? (float3)target.position : float3.zero;

            using (Markers.OnBeforePositionProjecting.Auto())
            {
                foreach (var component in m_Components)
                {
                    if (component is ISplitScreenTargetPosition modifier && component.isActiveAndEnabled)
                    {
                        positionWS = modifier.OnSplitScreenTargetPosition(screenIndex, positionWS);
                    }
                }
            }

            if (target == null)
                return float3.zero;

            Matrix4x4 viewMatrix = m_WorldToPlane;
            float3 positionPS = viewMatrix.MultiplyPoint(positionWS);

            return new float3(positionPS.x, positionPS.y, positionPS.z);
        }

        float3 GetScreenTranslatingPositionPS(int screenIndex)
        {
            var target = Screens[screenIndex].Target;

            var positionWS = target != null ? (float3)target.position : float3.zero;

            using (Markers.OnBeforePositionProjecting.Auto())
            {
                foreach (var component in m_Components)
                {
                    if (component is ISplitScreenTranslatingPosition modifier && component.isActiveAndEnabled)
                    {
                        positionWS = modifier.OnSplitScreenTranslatingPosition(screenIndex, positionWS);
                    }
                }
            }

            if (target == null)
                return float3.zero;

            Matrix4x4 viewMatrix = m_WorldToPlane;
            float3 positionPS = viewMatrix.MultiplyPoint(positionWS);

            return new float3(positionPS.x, positionPS.y, positionPS.z);
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            // It is here for the future so it would be easier to upgrade
            if (m_Version != 0)
            {
                m_Version = 0;
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckIsCreated()
        {
            if (!IsCreated)
                throw new Exception("Split screen effect must be created. This can happen if component is disabled or correct execution order is required.");
        }
    }
}
