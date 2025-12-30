// using System.Reflection;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using UnityEngine.Tilemaps;
//
// namespace ShadowModule
// {
//     using UnityEngine.Experimental.Rendering.Universal;
//     using System.Reflection;
//
//     public class AddShadowCaster : MonoBehaviour
//     {
//         private static BindingFlags accessFlagsPrivate =
//             BindingFlags.NonPublic | BindingFlags.Instance;
//
//         private static FieldInfo meshField =
//             typeof(ShadowCaster2D).GetField("m_Mesh", accessFlagsPrivate);
//
//         private static FieldInfo shapePathField =
//             typeof(ShadowCaster2D).GetField("m_ShapePath", accessFlagsPrivate);
//
//         private static MethodInfo onEnableMethod =
//             typeof(ShadowCaster2D).GetMethod("OnEnable", accessFlagsPrivate);
//
//         private PolygonCollider2D Polygon;
//
//         private ShadowCaster2D shadowCaster;
//
// // Start is called before the first frame update
//
//         void Start()
//
//         {
//             shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
//
//             Polygon = GetComponent<PolygonCollider2D>();
//
//             shadowCaster = GetComponent<ShadowCaster2D>();
//
//             shadowCaster.selfShadows = true;
//
//             Vector3[] positions = new Vector3[Polygon.GetTotalPointCount()];
//
//             for (int i = 0; i < Polygon.GetTotalPointCount(); i++)
//             {
//                 positions[i] = new Vector3(Polygon.points[i].x, Polygon.points[i].y, 0);
//             }
//
//             shapePathField.SetValue(shadowCaster, positions);
//
//             meshField.SetValue(shadowCaster, null);
//
//             onEnableMethod.Invoke(shadowCaster, new object[0]);
//         }
//     }
// }