using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProjectDawn.SplitScreen.Editor
{
    [CustomPropertyDrawer(typeof(ReloadAttribute))]
    public class ReloadDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ReloadProperty(property);
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
            ReloadProperty(property);
        }

        void ReloadProperty(SerializedProperty property)
        {
            if (property.objectReferenceValue == null)
            {
                var reloadAttribute = attribute as ReloadAttribute;
                switch (reloadAttribute.Scope)
                {
                    case ReloadScope.Default:
                        property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(reloadAttribute.Path);
                        break;
                    
                    case ReloadScope.BuiltinResources:
                        property.objectReferenceValue = Resources.GetBuiltinResource(GetBuiltinPropertyType(property), reloadAttribute.Path);
                        break;

                    case ReloadScope.BuiltinExtraResources:
                        property.objectReferenceValue = AssetDatabase.GetBuiltinExtraResource<Object>(reloadAttribute.Path);
                        break; 
                }
            }
        }

        static System.Type GetBuiltinPropertyType(SerializedProperty property)
        {
            // This code works under assumption that SerializedProperty type is in same assembly as Object
            string objectAssemblyQualifiedName = typeof(Object).AssemblyQualifiedName;
            string propertyTypeName = property.type.Replace("PPtr<$", "").Replace(">", "");
            string propertyAssemblyQualifiedName = objectAssemblyQualifiedName.Replace(typeof(Object).Name, propertyTypeName);
            return System.Type.GetType(propertyAssemblyQualifiedName);
        }
    }
}
