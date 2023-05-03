using UnityEngine;
using UnityEditor;

namespace ProjectDawn.SplitScreen.Editor
{
    [CustomPropertyDrawer(typeof(MaxValue))]
    public class MaxValueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var maxValueAttribute = attribute as MaxValue;

            float propertyValue = property.floatValue;
            EditorGUI.BeginChangeCheck();
            propertyValue = EditorGUI.FloatField(position, label, propertyValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = Mathf.Max(maxValueAttribute.Value, propertyValue);
            }
        }
    }
}
