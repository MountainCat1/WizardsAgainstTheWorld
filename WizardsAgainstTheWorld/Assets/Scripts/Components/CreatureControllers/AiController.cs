using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Pathfinding;
using UnityEngine;
using Zenject;

namespace CreatureControllers
{
    [RequireComponent(typeof(Seeker))]
    public abstract class AiController : CreatureController
    {
        public const float ThinkInterval = 0.25f;
        
        public event Action<IEnumerable<Vector3>> PathChanged;
        public event Action MemoryUpdated;

        protected Seeker Seeker { get; private set; }

        // Private Variables
        private const float MemoryUpdateInterval = 1.0f; // Base interval in seconds

        // Events

        // Injected Dependencies (using Zenject)
        [Inject] protected IPathfinding Pathfinding;
        [Inject] protected ICreatureManager CreatureManager;
        [Inject] protected IEntityManager EntityManager;

        // Public Constants
        private const double MemoryTime = 60;

        // Private Variables
        private Dictionary<Entity, long> _memorizedEntities = new();
        private NavigationCache _navigationCache;

        // Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            Seeker = GetComponent<Seeker>();
            if (Seeker == null)
            {
                GameLogger.LogError("Seeker component is missing on the Creature.");
            }

            _navigationCache = new NavigationCache(Seeker);

            PathChanged += path =>
            {
                if (!path.Any())
                {
                    UpdateMemory();
                }
            };
        }

        protected virtual void Start()
        {
            Creature.Health.Hit += OnHit;

            // Add a random offset to spread updates
            var randomOffset = UnityEngine.Random.Range(0f, MemoryUpdateInterval);
            StartCoroutine(UpdateMemoryPeriodically(randomOffset));
        }

        // Public Methods
        public IEnumerable<Entity> GetMemorizedEntities()
        {
            return _memorizedEntities
                .Select(x => x.Key)
                .Where(x => x);
        }
        public IEnumerable<Creature> GetMemorizedCreatures()
        {
            return _memorizedEntities
                .Select(x => x.Key)
                .Where(x => x)
                .OfType<Creature>();
        }

        protected void PerformMovementTowardsTarget(Entity target)
        {
            PerformMovementTowardsPosition(target.transform.position);
        }

        // Private Methods
        private void MoveViaPathfinding(Vector2 targetPosition)
        {
            if (Seeker == null)
            {
                GameLogger.LogError("Seeker is not assigned.");
                return;
            }

            // Request a path from the current position to the target
            _navigationCache.StartPath(Creature.transform.position, targetPosition, OnPathComplete);
        }

        private void OnPathComplete(Path p)
        {
            if (p.error || p.vectorPath.Count == 0)
            {
                GameLogger.LogError("Pathfinding failed or returned an empty path.");
                Creature.SetMovement(Vector2.zero);
                return;
            }

            // Get the next waypoint in the path
            var nextNode = p.vectorPath[1]; // [0] is the current position
            Vector2 direction = ((Vector3)nextNode - Creature.transform.position).normalized;

            if (Vector2.Distance(Creature.transform.position, nextNode) < 0.1f)
            {
                Creature.SetMovement(Vector2.zero);

                PathChanged?.Invoke(Enumerable.Empty<Vector3>());

                return;
            }

            // Move the creature in the direction of the next node
            Creature.SetMovement(direction);

            // Draw debug lines for the path
            Debug.DrawLine(Creature.transform.position, nextNode, Color.red);
            for (int i = 0; i < p.vectorPath.Count - 1; i++)
            {
                Debug.DrawLine(p.vectorPath[i], p.vectorPath[i + 1], Color.green);
            }

            // Check collision with the next node
            // TODO: Performance optimization needed
            // This is here so that if we click off the map the pathfinding want to go at the edge of walkable area
            // on which if creature will stand it would be overlapping with the wall collider
            // Which ends up in it walking into a wall indefinitely
            // Alternatively we could check it more rarely,
            // or make the path update less often
            // or check for like 0.3s if the creature is not moving and then just make it stop
            if (Vector2.Distance(Creature.transform.position, nextNode) < Creature.ColliderSize)
            {
                if (Physics2D.OverlapCircle(nextNode, Creature.Movement.Collider.radius, LayerMask.GetMask("Walls")))
                {
                    Creature.SetMovement(Vector2.zero);
                    PathChanged?.Invoke(Array.Empty<Vector3>());
                    return;
                }
            }

            PathChanged?.Invoke(p.vectorPath);
        }

        private void MoveStraightToTarget(Vector2 targetPosition)
        {
            var direction = (targetPosition - (Vector2)Creature.transform.position).normalized;

            if (Vector2.Distance(Creature.transform.position, targetPosition) < 0.1f)
            {
                PathChanged?.Invoke(Array.Empty<Vector3>());
                Creature.SetMovement(Vector2.zero);
                return;
            }

            // Check collision with the next node
            // TODO: Performance optimization needed
            // This is here so that if we click off the map the pathfinding want to go at the edge of walkable area
            // on which if creature will stand it would be overlapping with the wall collider
            // Which ends up in it walking into a wall indefinitely
            // Alternatively we could check it more rarely,
            // or make the path update less often
            // or check for like 0.3s if the creature is not moving and then just make it stop
            if (Vector2.Distance(Creature.transform.position, targetPosition) < Creature.ColliderSize)
            {
                if (Physics2D.OverlapCircle(targetPosition, Creature.Movement.Collider.radius,
                        LayerMask.GetMask("Walls")))
                {
                    Creature.SetMovement(Vector2.zero);
                    PathChanged?.Invoke(Array.Empty<Vector3>());
                    return;
                }
            }

            PathChanged?.Invoke(new Vector3[] { transform.position, targetPosition });

            Creature.SetMovement(direction);
            Debug.DrawLine(Creature.transform.position, targetPosition, Color.green);
        }

        protected List<Vector3> GetCornerPoints(Vector3 center, float radius)
        {
            List<Vector3> cornerPoints = new List<Vector3>
            {
                center + new Vector3(radius, 0, 0), // Right
                center + new Vector3(0, radius, 0), // Up
                center + new Vector3(-radius, 0, 0), // Left
                center + new Vector3(0, -radius, 0) // Down
            };
            return cornerPoints;
        }

        protected virtual void UpdateMemory()
        {
            long currentTicks = Environment.TickCount;
            var keysToRemove = new List<Entity>();

            foreach (var kvp in _memorizedEntities)
            {
                if ((currentTicks - kvp.Value) > MemoryTime * 1000 || !kvp.Key)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _memorizedEntities.Remove(key);
            }

            foreach (var creature in CreatureManager.GetCreaturesAliveActive())
            {
                if (CanSee(creature))
                {
                    Memorize(creature);
                }
            }

            MemoryUpdated?.Invoke();
        }

        private IEnumerator UpdateMemoryPeriodically(float randomOffset)
        {
            // Initial delay to stagger updates
            yield return new WaitForSeconds(randomOffset);

            while (true)
            {
                UpdateMemory();
                yield return new WaitForSeconds(MemoryUpdateInterval);
            }

            // ReSharper disable once IteratorNeverReturns
        }

        // Event Handlers
        private void OnHit(HitContext ctx)
        {
            if (ctx.Attacker is not null)
                Memorize(ctx.Attacker);
        }

        // Helper Methods
        protected bool PathClear(Entity target, float radius)
        {
            return PathClear(target.transform.position, radius);
        }

        protected bool PathClear(Vector2 targetPosition, float radius)
        {
            Vector3 creaturePosition = Creature.transform.position;
            List<Vector3> cornerPoints = GetCornerPoints(creaturePosition, radius);

            bool pathClear = true;
            foreach (Vector3 corner in cornerPoints)
            {
                if (!Pathfinding.IsClearPath(corner, targetPosition))
                    pathClear = false;
            }

            return pathClear;
        }

        public void Memorize(Entity creature)
        {
            _memorizedEntities[creature] = Environment.TickCount;
        }

        protected void PerformMovementTowardsPosition(Vector2 position)
        {
            float radius = Creature.Movement.Collider.radius;

            if (PathClear(position, radius))
            {
                foreach (Vector3 corner in GetCornerPoints(Creature.transform.position, radius))
                {
                    Debug.DrawLine(corner, position, Color.blue);
                }

                MoveStraightToTarget(position);
            }
            else
            {
                MoveViaPathfinding(position);
            }
        }

        
        // TODO: OPTIMIZE THIS SHIT
        protected Entity GetNewTarget()
        {
            var targets = GetMemorizedEntities()
                .Where(x => Creature.GetAttitudeTowards(x) == Attitude.Hostile)
                .Where(x => EntityManager.IsAliveAndActive(x));

            return targets
                .OrderBy(x => Vector2.Distance(Creature.transform.position, x.transform.position))
                .FirstOrDefault();
        }

        protected bool IsInRange(Entity creature, float range)
        {
            return Vector2.Distance(Creature.transform.position, creature.transform.position) < range;
        }

        protected void InvokePathChanged(IEnumerable<Vector2> path)
        {
            PathChanged?.Invoke(path.Select(x => (Vector3)x));
        }

        protected void ClearMemory()
        {
            _memorizedEntities.Clear();
        }
    }
}