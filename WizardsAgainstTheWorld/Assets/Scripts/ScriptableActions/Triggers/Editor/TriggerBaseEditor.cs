using UnityEditor;
using UnityEngine;
using Triggers;

[CustomEditor(typeof(TriggerBase), true)] // Ensures it works for derived classes
public class TriggerBaseEditor : Editor
{
    private SerializedProperty actionsProp;
    private SerializedProperty conditionsProp;
    private SerializedProperty fireOnceProp;
    private SerializedProperty actionObjectProp;

    private void OnEnable()
    {
        // Using backing field names for [field: SerializeField] properties
        actionsProp = serializedObject.FindProperty("<Actions>k__BackingField");
        conditionsProp = serializedObject.FindProperty("<Conditions>k__BackingField");
        fireOnceProp = serializedObject.FindProperty("<FireOnce>k__BackingField");
        actionObjectProp = serializedObject.FindProperty("actionObject");
        
        // Fallback for potential hidden fields
        if (actionObjectProp == null)
            actionObjectProp = serializedObject.FindProperty("<actionObject>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except Actions & Conditions initially
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Skip Actions & Conditions for now; we'll handle them later
            if (iterator.name == "Actions" || iterator.name == "Conditions")
                continue;

            EditorGUILayout.PropertyField(iterator, true);
        }

        // If actionObject is false, show Actions & Conditions
        if (actionObjectProp != null && !actionObjectProp.boolValue)
        {
            if (actionsProp != null)
                EditorGUILayout.PropertyField(actionsProp, true);
            if (conditionsProp != null)
                EditorGUILayout.PropertyField(conditionsProp, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
