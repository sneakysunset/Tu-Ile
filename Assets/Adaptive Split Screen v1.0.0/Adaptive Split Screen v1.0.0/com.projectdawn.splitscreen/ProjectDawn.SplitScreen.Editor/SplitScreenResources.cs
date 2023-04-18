using UnityEngine;
using UnityEditor;

namespace ProjectDawn.SplitScreen.Editor
{
    /// <summary>
    /// For accessing default resources of split screen.
    /// </summary>
    public static class SplitScreenResources
    {
        const string DefaultMaterialPath = "Packages/com.projectdawn.splitscreen/ProjectDawn.SplitScreen/Default-SplitScreen.mat";

        /// <summary>
        /// Default blit material for split screen.
        /// </summary>
        public static Material DefaultMaterial => Load<Material>(DefaultMaterialPath);

        static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                Debug.LogError($"Could not find default material at path ({DefaultMaterialPath})!");
            return asset;
        }
    }
}
