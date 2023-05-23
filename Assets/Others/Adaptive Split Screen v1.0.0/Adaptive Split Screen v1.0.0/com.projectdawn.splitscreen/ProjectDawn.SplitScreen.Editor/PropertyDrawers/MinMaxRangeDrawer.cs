using UnityEngine;
using UnityEditor;

namespace ProjectDawn.SplitScreen.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxRange))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMaxRangeAttribute = attribute as MinMaxRange;

            Vector2 propertyValue = property.vector2Value;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref propertyValue.x, ref propertyValue.y, minMaxRangeAttribute.Min, minMaxRangeAttribute.Max);
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = propertyValue;
            }
        }
    }
}
