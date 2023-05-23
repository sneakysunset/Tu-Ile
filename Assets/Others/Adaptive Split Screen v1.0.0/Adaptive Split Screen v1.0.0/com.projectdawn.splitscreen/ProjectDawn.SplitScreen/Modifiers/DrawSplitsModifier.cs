using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen
{
    /// <summary>
    /// Draws split lines.
    /// </summary>
    public class DrawSplitsModifier : MonoBehaviour, ISplitScreenCommandBuffer
    {
        [Reload("Packages/com.projectdawn.splitscreen/Samples/Materials/Default-SplitLine.mat")]
        public Material LineMaterial;
        [Reload("Packages/com.projectdawn.splitscreen/Samples/Materials/Default-SplitCircle.mat")]
        public Material CircleMaterial;
        [Reload("Quad.fbx", ReloadScope.BuiltinResources)]
        public Mesh Mesh;
        public float Width = 15;

        void OnEnable() {} // This will force component enable/disable to show up

        public void OnSplitScreenCommandBuffer(CommandBuffer cmd, in ScreenRegions screenRegions)
        {
            if (LineMaterial == null || CircleMaterial == null || Mesh == null)
                return;

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, float4x4.Ortho(Screen.width, Screen.height, 0, 1));

            var scale = new float2(Screen.width, Screen.height) / 2f;
            foreach (var split in screenRegions.Splits)
            {
                var startSS = split.Line.From * scale;
                var endSS = split.Line.To * scale;
                DrawLine(cmd, new Line(startSS, endSS), split.Blend * Width);
                DrawCircle(cmd, startSS, split.Blend * Width);
                DrawCircle(cmd, endSS, split.Blend * Width);
            }
        }

        void DrawLine(CommandBuffer cmd, Line line, float width)
        {
            float2 direction = line.Direction;
            float angle = math.atan2(direction.y, direction.x);
            quaternion rotation = quaternion.RotateZ(angle);

            float3 center = new float3(line.MidPoint, 0);
            float length = line.Length;

            float4x4 matrix = float4x4.TRS(center, rotation, new float3(length, width, 1));
            cmd.DrawMesh(Mesh, matrix, LineMaterial, 0, 0);
        }

        void DrawCircle(CommandBuffer cmd, float2 point, float width)
        {
            float4x4 matrix = float4x4.TRS(new float3(point, 0), quaternion.identity, new float3(width, width, 1));
            cmd.DrawMesh(Mesh, matrix, CircleMaterial, 0, 0);
        }
    }
}
