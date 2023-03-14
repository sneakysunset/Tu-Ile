using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class HideNormalInspector : UnityEngine.PropertyAttribute
{
    public HideNormalInspector()
    {
    }


}

[CustomPropertyDrawer(typeof(HideNormalInspector))]
public class HideInNormalInspectorDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -2; // To compensate the gap between inspector's properties
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

    }
}
