using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
    public static class PointerUtilities
    {
        public static bool IsPointerOverInteractiveUI(params GameObject[] ignoreObjects)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            // Filter out the selection box or other non-interactive elements
            foreach (var result in results)
            {
                if(ignoreObjects.Contains(result.gameObject))
                {
                    // Ignore the selection box
                    continue;
                }
                
                if(result.gameObject.layer == LayerMask.NameToLayer("In World UI"))
                {
                    // Ignore the UI
                    continue;
                }
                
                if(result.gameObject.CompareTag("TransparentUI"))
                {
                    // Ignore the in-world UI
                    continue;
                }
                
                if(!result.gameObject.activeInHierarchy)
                    continue;

                return true;
            }

            return false;
        }
    }
}