using System;
using Pathfinding;
using UnityEngine;

namespace CreatureControllers
{
    public class NavigationCache
    {
        public event Action<Path> PathComplete;

        private Seeker _seeker;
        private float _time;
        private float _cacheTime;

        public NavigationCache(Seeker seeker, float cacheTime = 0.5f)
        {
            _seeker = seeker;
            _cacheTime = cacheTime;
        }

        public void StartPath(Vector2 position, Vector2 destination, Action<Path> callback)
        {
            if (Time.time - _time < _cacheTime)
            {
                if (_seeker.IsDone())
                {
                    // Maybe we should for the end of frame???
                    OnPathComplete(_seeker.GetCurrentPath());
                }

                return;
            }

            _seeker.StartPath(position, destination, (path) =>
            {
                callback?.Invoke(path);
            });
        }

        public void Invalidate()
        {
            _seeker.CancelCurrentPathRequest();
            _time = 0;
        }

        private void OnPathComplete(Path p)
        {
            if (p.error || p.vectorPath.Count == 0)
            {
                GameLogger.LogError("Pathfinding failed or returned an empty path.");
                return;
            }

            PathComplete?.Invoke(p);
        }
    }
}