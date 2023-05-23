using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

namespace ProjectDawn.SplitScreen.Editor
{
    static class MenuItemSplitScreen
    {
        const string DefaultMaterialPath = "Packages/com.projectdawn.splitscreen/ProjectDawn.SplitScreen/Blit.mat";

        [MenuItem("GameObject/Rendering/Split Screen", false, 10)]
        static void Create(MenuCommand menuCommand)
        {
            // Create a custom game object
            var gameObject = new GameObject("Split Screen");

            var camera = gameObject.AddComponent<Camera>();

            camera.orthographic = true;
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.backgroundColor = Color.black;

            var splitScreen = gameObject.AddComponent<SplitScreenEffect>();

            splitScreen.Material = SplitScreenResources.DefaultMaterial;

            float distance = 5;

            CreateScreen(splitScreen, "A", PlayerColor.PlayerA, new float3(-distance, -distance, 0));
            CreateScreen(splitScreen, "B", PlayerColor.PlayerB, new float3(distance, -distance, 0));
            CreateScreen(splitScreen, "C", PlayerColor.PlayerC, new float3(distance, distance, 0));
            CreateScreen(splitScreen, "D", PlayerColor.PlayerD, new float3(-distance, distance, 0));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
            Selection.activeObject = gameObject;
        }

        static void CreateScreen(SplitScreenEffect splitScreen, string name, Color clearColor, float3 position)
        {
            var childGameObject = new GameObject($"Screen {name}");
            childGameObject.transform.SetParent(splitScreen.transform);
            var childCamera = childGameObject.AddComponent<Camera>();
            childCamera.clearFlags = CameraClearFlags.SolidColor;
            childCamera.backgroundColor = clearColor;

            var childTarget = new GameObject($"Target {name}");
            childTarget.transform.SetParent(splitScreen.transform);
            childTarget.transform.localPosition = position;

            splitScreen.Screens.Add(new ScreenData
            {
                Camera = childCamera,
                Target = childTarget.transform,
            });
        }
    }
}
