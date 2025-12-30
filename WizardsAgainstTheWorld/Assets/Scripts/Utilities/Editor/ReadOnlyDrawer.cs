#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Utilities.Editor
{
    [CustomPropertyDrawer(typeof(Utilities.ReadOnlyInInspectorAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false; // Disable editing
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true; // Re-enable editing
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

#endif