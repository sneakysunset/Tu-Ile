using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditorInternal;

namespace ProjectDawn.SplitScreen.Editor
{
    [CustomEditor(typeof(SplitScreenEffect))]
    [CanEditMultipleObjects]
    public class SplitScreenEffectEditor : UnityEditor.Editor
    {
        public static class Styles
        {
            public static readonly GUIContent Material = EditorGUIUtility.TrTextContent("Material", "Material used for drawing each screen region.");
            public static readonly GUIContent Distance = EditorGUIUtility.TrTextContent("Distance", "Distance from the each screen target in world space.");
            public static readonly GUIContent BoundsRadius = EditorGUIUtility.TrTextContent("Bounds Radius", "Radius that is used for construcing bounds.");

        }

        SerializedProperty m_Material;
        SerializedProperty m_Distance;
        SerializedProperty m_BoundsRadius;
        SerializedProperty m_Translating;
        SerializedProperty m_Balancing;
        SerializedProperty m_BalancingEnabled;
        SerializedProperty m_BalancingError;
        SerializedProperty m_BalancingRelaxationIterations;
        SerializedProperty m_UseAspectRatio;
        SerializedProperty m_DrawFlags;

        SerializedProperty m_Screens;
        ReorderableList m_ScreensList;
        MaterialEditor m_MaterialEditor;
        bool m_IsDefaultMaterial;

        public override void OnInspectorGUI()
        {
            var splitScreen = target as SplitScreenEffect;

            EditorGUILayout.BeginVertical();

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Material, Styles.Material);
            if (EditorGUI.EndChangeCheck())
            {
                RecreateMaterialEditor(m_Material.objectReferenceValue as Material);
            }

            EditorGUILayout.PropertyField(m_Distance, Styles.Distance);
            EditorGUILayout.PropertyField(m_BoundsRadius, Styles.BoundsRadius);
            EditorGUILayout.PropertyField(m_Translating);

            using (new EditorGUI.DisabledScope(m_Screens.arraySize < 3))
            {
                if (EditorGUILayout.PropertyField(m_Balancing, false))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_BalancingEnabled);
                    using (new EditorGUI.DisabledScope(!m_BalancingEnabled.boolValue))
                    {
                        if (m_Screens.arraySize == 4)
                        {
                            EditorGUILayout.PropertyField(m_BalancingRelaxationIterations);
                        }

                        // Draws the balance error
                        using (new EditorGUI.DisabledScope(true))
                            EditorGUILayout.FloatField("Error", splitScreen.GetBalanceError());

                        if (m_BalancingEnabled.boolValue)
                        {
                            EditorGUILayout.HelpBox("Balancing for 4 sites is currently experimental, might contain erratic movement.", MessageType.Info);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.PropertyField(m_UseAspectRatio);
            EditorGUILayout.PropertyField(m_DrawFlags);

            EditorGUILayout.Space();
            m_ScreensList.DoLayoutList();
            EditorGUILayout.Space();

            OnValidate();

            Validate(splitScreen);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();

            if (m_MaterialEditor != null)
            {
                EditorGUILayout.Space();
                
                using (new EditorGUI.DisabledGroupScope(m_IsDefaultMaterial))
                {
                    m_MaterialEditor.DrawHeader();
                    m_MaterialEditor.OnInspectorGUI();
                }
            }
        }

        protected virtual void OnValidate()
        {
            if (QualitySettings.renderPipeline != null || GraphicsSettings.renderPipelineAsset != null)
                EditorGUILayout.HelpBox("Unsupported version of render pipeline is currently used.", MessageType.Warning);
        }

        void Validate(SplitScreenEffect splitScreen)
        {
            var camera = splitScreen.GetComponent<Camera>();

            var eulerAngles = camera.transform.rotation.eulerAngles;
            if (!camera.orthographic && (eulerAngles.y != 0 || eulerAngles.z != 0))
            {
                DrawFixMeBox("Currently perspective camera only support rotation around x axis. This will be changed in the future. ", MessageType.Error, () =>
                {
                    camera.transform.rotation = Quaternion.Euler(eulerAngles.x, 0, 0);
                });
                return;
            }

            if (m_Screens.arraySize > 4)
            {
                EditorGUILayout.HelpBox($"Only supports up to 4 split screens.", MessageType.Warning);
                return;
            }

            if (camera.clearFlags != CameraClearFlags.Nothing)
            {
                DrawFixMeBox("It is recommended to set camera clear flag to nothing for better performance.", MessageType.Info, () =>
                {
                    camera.clearFlags = CameraClearFlags.Nothing;
                });
                return;
            }

            if (camera.cullingMask != 0)
            {
                DrawFixMeBox("It is recommended to set camera culling mask to nothing for better performance.", MessageType.Info, () =>
                {
                    camera.cullingMask = 0;
                });
                return;
            }
        }

        // UI Helpers
        /// <summary>Draw a Fix button</summary>
        /// <param name="text">Displayed message</param>
        /// <param name="action">Action performed when fix buttom is clicked</param>
        public static void DrawFixMeBox(string text, MessageType messageType, System.Action action)
        {
            EditorGUILayout.HelpBox(text, messageType);

            GUILayout.Space(-32);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Fix", GUILayout.Width(60)))
                    action();

                GUILayout.Space(8);
            }
            GUILayout.Space(11);
        }

        void OnEnable()
        {
            m_Material = serializedObject.FindProperty("Material");
            m_Distance = serializedObject.FindProperty("Distance");
            m_BoundsRadius = serializedObject.FindProperty("BoundsRadius");
            m_Balancing = serializedObject.FindProperty("Balancing");
            m_BalancingEnabled = m_Balancing.FindPropertyRelative("Enabled");
            m_BalancingError = m_Balancing.FindPropertyRelative("Error");
            m_BalancingRelaxationIterations = m_Balancing.FindPropertyRelative("RelaxationIterations");
            m_Translating = serializedObject.FindProperty("Translating");
            m_UseAspectRatio = serializedObject.FindProperty("UseAspectRatio");
            m_DrawFlags = serializedObject.FindProperty("DrawFlags");
            m_Screens = serializedObject.FindProperty("Screens");
            RecreateMaterialEditor(m_Material.objectReferenceValue as Material);

            m_ScreensList = new ReorderableList(serializedObject, m_Screens, false, true, true, true);
            m_ScreensList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 10;
            m_ScreensList.drawElementCallback = DrawListItems;
            m_ScreensList.drawHeaderCallback = DrawHeader;
            m_ScreensList.onCanAddCallback = (ReorderableList list) =>
            {
                return list.count < 4;
            };
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_ScreensList.serializedProperty.GetArrayElementAtIndex(index); // The element in the list

            float y = rect.y + 3.5f;

            EditorGUI.PropertyField(
                new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), 
                element.FindPropertyRelative("Camera"), EditorGUIUtility.TrTextContent($"Camera {index}"), false
            );
            y += EditorGUIUtility.singleLineHeight + 4f;

            EditorGUI.PropertyField(
                new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight), 
                element.FindPropertyRelative("Target"), EditorGUIUtility.TrTextContent($"Target {index}"), false
            ); 
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Screens");
        }

        void OnDisable()
        {
            if (m_MaterialEditor != null)
                DestroyImmediate(m_MaterialEditor);
        }

        void RecreateMaterialEditor(Material material)
        {
            if (m_MaterialEditor != null)
                DestroyImmediate(m_MaterialEditor);
            if (material != null)
            {
                m_MaterialEditor = (MaterialEditor) CreateEditor(material);
                m_IsDefaultMaterial = material == SplitScreenResources.DefaultMaterial;
            }
            else
                m_MaterialEditor = null;
        }
    }
}
