using UnityEngine;
using System.Reflection;
using UnityEngine.Rendering.Universal;

public static class ShadowMorpherUtil
{
    private static BindingFlags accessFlagsPrivate = BindingFlags.NonPublic | BindingFlags.Instance;

    private static FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
    private static FieldInfo shapePathHash = typeof(ShadowCaster2D).GetField("m_ShapePathHash", accessFlagsPrivate);

    private static Assembly lightUtilityAssembly = typeof(Light2D).Assembly;
    private static System.Type lightUtilityType = lightUtilityAssembly.GetType("UnityEngine.Rendering.Universal.LightUtility");

    private static MethodInfo getShapeHashMethod = lightUtilityType.GetMethod("GetShapePathHash");

    public static void SetShadowShape(ShadowCaster2D shadowCaster, Vector3[] polygonPoints)
    {
        shapePathField.SetValue(shadowCaster, polygonPoints);

        object shapePathFieldValue = shapePathField.GetValue(shadowCaster);
        object shapeHash = getShapeHashMethod.Invoke(null, new object[] { shapePathFieldValue });
        shapePathHash.SetValue(shadowCaster, shapeHash);
    }

    public static void SetShape(this ShadowCaster2D shadowCaster, Vector3[] polygonPoints) =>
        SetShadowShape(shadowCaster, polygonPoints);
}