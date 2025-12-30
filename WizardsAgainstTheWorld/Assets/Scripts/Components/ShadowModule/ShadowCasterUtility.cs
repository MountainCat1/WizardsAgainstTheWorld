using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShadowModule
{
    public static class ShadowCasterUtility
    {
        private static readonly BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly FieldInfo shapePathField =
            typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);

        private static readonly FieldInfo forceRebuildField =
            typeof(ShadowCaster2D).GetField("m_ForceShadowMeshRebuild", accessFlagsPrivate);

        private static readonly MethodInfo onEnableMethod =
            typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);

        public static ShadowCaster2D UpdateShadowCasterShape(GameObject target, Vector2[] positions, bool selfShadows = true)
        {
            if (target == null || positions == null || positions.Length == 0)
            {
                GameLogger.LogWarning("ShadowCasterUtility: Invalid target or positions array.");
                return null;
            }

            var shadowCaster = target.GetComponent<ShadowCaster2D>() ?? target.AddComponent<ShadowCaster2D>();
            shadowCaster.selfShadows = selfShadows;

            if (shapePathField != null)
                shapePathField.SetValue(shadowCaster, positions.Select(x => new Vector3(x.x, x.y, 0f)).ToArray());

            if (forceRebuildField != null)
                forceRebuildField.SetValue(shadowCaster, true);

            if (onEnableMethod != null)
                onEnableMethod.Invoke(shadowCaster, null);

            return shadowCaster;
        }

    }
}