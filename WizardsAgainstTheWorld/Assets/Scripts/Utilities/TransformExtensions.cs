using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            var result = parent.Find(name);
            if (result != null)
                return result;
            foreach (Transform child in parent)
            {
                result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static List<Transform> GetAllChildrenTransforms(this Transform parent)
        {
            var transforms = new List<Transform>();
            AddChildrenRecursive(parent, transforms);
            return transforms;
        }

        private static void AddChildrenRecursive(Transform current, List<Transform> list)
        {
            list.Add(current);
            for (int i = 0; i < current.childCount; i++)
            {
                AddChildrenRecursive(current.GetChild(i), list);
            }
        }

        public static List<GameObject> GetDirectChildrenIncludingInactive(this Transform parent)
        {
            List<GameObject> result = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                result.Add(child.gameObject);
            }

            return result;
        }
    }
}