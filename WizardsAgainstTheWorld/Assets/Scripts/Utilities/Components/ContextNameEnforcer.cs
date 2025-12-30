using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    [ExecuteAlways]
    public class ContextNameEnforcer : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            // Skip during play mode
            if (Application.isPlaying) return;

            EnforceContextNames();
        }

        private void EnforceContextNames()
        {
            foreach (var child in transform.GetAllChildrenTransforms())
            {
                // Don't rename the root itself
                if (child == transform) continue;

                var firstComponent = child.GetComponents<Component>()
                    .FirstOrDefault(c => c != null && !(c is Transform));

                if (firstComponent == null) continue;

                var contextName = firstComponent.GetType().Name;
                var go = child.gameObject;

                if (!go.name.StartsWith(contextName + "_"))
                {
                    Undo.RecordObject(go, "Enforce Context Name");
                    go.name = $"{contextName}";
                }
            }
        }
#endif
    }
}