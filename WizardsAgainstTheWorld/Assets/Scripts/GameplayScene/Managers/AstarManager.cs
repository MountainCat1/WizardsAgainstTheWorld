using System.Collections;
using Pathfinding;
using UnityEngine;

namespace Managers
{
    public interface IAstarManager
    {
        void Scan();
        void ScanDelayed();
        Vector2 GetClosestValidPosition(Vector2 position);
    }

    public class AstarManager : MonoBehaviour, IAstarManager
    {
        [SerializeField] private AstarPath astarPath;

        private bool _scheduledForScan = false;
        
        public void Scan()
        {
            GameLogger.Log("Calling astarPath.Scan()");
            AstarPath.active.Scan();
        }

        public void ScanDelayed()
        {
            if(_scheduledForScan)
                return;
            
            _scheduledForScan = true;
            StartCoroutine(ScanDelayedCoroutine());
        }
        
        public Vector2 GetClosestValidPosition(Vector2 position)
        {
            // Query nearest node from A* Pathfinding
            var nearest = AstarPath.active.GetNearest(position, NNConstraint.Walkable);
            if (nearest.node == null || !nearest.node.Walkable)
            {
                // If the nearest node is invalid, fall back to original pos
                GameLogger.LogWarning($"No valid node found near {position}, returning input position.");
                return position;
            }

            return (Vector3)nearest.position;
        }


        private IEnumerator ScanDelayedCoroutine()
        {
            yield return new WaitForFixedUpdate();
            
            _scheduledForScan = false;
            
            // we do this shittery so that obstacles can call it in a way that the scan will happened after they will get destroyed
            // coz destroying something only marks is for destruction, it does not destroy it immediately,
            // and the destruction will happen in the next frame
            // so we need to wait for the next frame to scan
            
            GameLogger.Log("Calling astarPath.ScanDelayed()");
            AstarPath.active.Scan();
        }
    }
}