using System.Collections;
using System.Linq;
using UnityEngine;

namespace CreatureControllers
{
    public class UnitController : AiController
    {
        public IInteractable InteractionTarget => _interactionTarget;
        public Vector2? MoveCommandTarget => _moveCommandTarget;

        private Vector2? _moveCommandTarget;
        protected Entity Target;
        private IInteractable _interactionTarget;
        private Interaction _interaction;

        private const bool MoveOnAttackCooldown = false; // TODO: This should be a configurable property of the weapon

        // It does seem that for player creatures, it is more intuitive for units to not follow the targets
        [SerializeField] private bool followTarget = true;
        [SerializeField] private bool clearMemoryOnMoveCommand = true;

        [SerializeField] private bool useMoveTimeout = false;
        [SerializeField] private float moveTimeout = 0.5f;

        private Vector2 _lastPosition;
        private Vector2 _lastDirection;
        private float _lastMovedTime;

        // Public Methods
        public void SetMoveTarget(Vector2 target)
        {
            if (clearMemoryOnMoveCommand)
            {
                ClearMemory();
            }

            _moveCommandTarget = target;
            _lastMovedTime = Time.time;
            Target = null;
            _interactionTarget = null;
            _interaction?.Cancel();
            _interaction = null;
        }

        public void SetTarget(object target) // object??????
        {
            _interaction?.Cancel();

            switch (target)
            {
                case null:
                    Target = null;
                    _interactionTarget = null;
                    return;
                case Creature creature:
                    Target = creature;
                    _interactionTarget = null;
                    return;
                case IInteractable interactable:
                    Target = null;
                    if (interactable.IsInteractable)
                        _interactionTarget = interactable;
                    return;
                default:
                    Target = null;
                    _interactionTarget = null;
                    GameLogger.LogWarning($"Invalid target type {target.GetType()}");
                    return;
            }
        }

        // Protected Methods

        protected virtual void Think()
        {
            if (_interactionTarget != null)
            {
                HandleInteraction();
                return;
            }

            if (_moveCommandTarget.HasValue)
            {
                HandleMovementToTarget();
                return;
            }

            if (Creature.Weapon is null)
            {
                Creature.SetMovement(Vector2.zero);
                return;
            }

            if (Target && Target.gameObject.activeInHierarchy == false)
            {
                Target = null;
            }

            if (Target == null || !EntityManager.IsAliveAndActive(Target) == false)
            {
                Target = GetNewTarget();
            }

            if (Target != null)
            {
                HandleAttack();
                return;
            }

            if (Creature.Weapon.ReloadComponent is not null && Creature.Weapon.ReloadComponent.CanReload)
            {
                Creature.Weapon.ReloadComponent.DoReloading(Creature);
            }

            Creature.SetMovement(Vector2.zero);
        }

        protected override void Start()
        {
            base.Start();
            _lastPosition = Creature.transform.position;
            _lastMovedTime = Time.time;

            PathChanged += (path) =>
            {
                if (!path.Any())
                    _moveCommandTarget = null;
            };

            MemoryUpdated += () =>
            {
                if (Target is not null)
                {
                    // If there is a closer target, switch to it
                    var newTarget = GetNewTarget();

                    if (newTarget != Target)
                    {
                        SetTarget(newTarget);
                    }
                }
            };
        }

        private IEnumerator ThinkCoroutine()
        {
            while (true)
            {
                Think();
                yield return new WaitForEndOfFrame();
            }
        }

        private void Update()
        {
            Think();

            // Mostly only player controller units use the move timeout
            // so we don't want to run this math for all creatures
            if (useMoveTimeout)
            {
                TrackMovement();
                ResetMoveIfStuck();
            }
        }

        public void Halt()
        {
            ClearMemory();
            _moveCommandTarget = null;
            Target = null;
            _interactionTarget = null;
            _interaction?.Cancel();

            Creature.SetMovement(Vector2.zero);
            InvokePathChanged(Enumerable.Empty<Vector2>());
        }


        // Private Methods
        private void HandleInteraction()
        {
            if (!_interactionTarget.CanInteract(Creature))
            {
                _interactionTarget = null;
                return;
            }

            if (Vector2.Distance(Creature.transform.position, _interactionTarget.Position) < Creature.InteractionRange)
            {
                Creature.SetMovement(Vector2.zero);
                InvokePathChanged(Enumerable.Empty<Vector2>());

                var interaction = Creature.Interact(_interactionTarget);

                if (interaction.Status == InteractionStatus.Created)
                {
                    _moveCommandTarget = null;

                    interaction.Completed += () => { _interactionTarget = null; };

                    interaction.Canceled += () => { _interactionTarget = null; };

                    _interaction = interaction;
                }

                Creature.SetMovement(Vector2.zero);
                return;
            }
            else
            {
                PerformMovementTowardsPosition(_interactionTarget.Position);
            }
        }

        private void HandleAttack()
        {
            if (Target != null)
            {
                HandleAttackOrMovementToTarget();
                return;
            }

            Creature.SetMovement(Vector2.zero); // Stop moving if no target
        }

        private void HandleMovementToTarget()
        {
            if (Vector2.Distance(Creature.transform.position, _moveCommandTarget.Value) > 0.21f)
            {
                PerformMovementTowardsPosition(_moveCommandTarget.Value);
            }
            else
            {
                InvokePathChanged(Enumerable.Empty<Vector2>()); // Clear the path
                _moveCommandTarget = null; // Reached the destination
            }
        }

        private void HandleAttackOrMovementToTarget()
        {
            var attackContext = CreateAttackContext();

            if (Creature.Weapon.GetOnCooldown(attackContext) && !Creature.Weapon.AllowToMoveOnCooldown)
            {
                Creature.SetMovement(Vector2.zero); // Stay still during cooldown if configured
                return;
            }

            if (Creature.Weapon.NeedsLineOfSight)
            {
                // TODO: we replaced those two cases aka "not shooting through allies", and "moving when cannot see", to avoid a weird behaviour
                // when creatures get closer when in a line and the enemy gets pushed so even though they wont attack they come closer
                // and break the formation

                if (!Creature.Weapon.ShootThroughAllies &&
                    Vector2.Distance(Creature.transform.position, Target.transform.position) <
                    DistanceToIgnoreFriendlyFire)
                {
                    // Stay still if there are friendly creatures in the line of sight
                    var creaturesInLineOfFire = GetCreatureInLine(Target.transform.position, Creature.Weapon.Range);
                    if (creaturesInLineOfFire.Any(x => x.GetAttitudeTowards(Creature) == Attitude.Friendly))
                    {
                        Creature.SetMovement(Vector2.zero);
                        return;
                    }
                }

                // Move towards the target if it is not in line of sight
                if (!CanSee(Target))
                {
                    if (!ShouldFollowTarget())
                    {
                        Creature.SetMovement(Vector2.zero);
                        return;
                    }

                    PerformMovementTowardsTarget(Target);
                    return;
                }
            }


            if (Creature.Weapon.IsInRange(attackContext))
            {
                PerformAttack(attackContext);
                return;
            }

            if (!ShouldFollowTarget())
            {
                Creature.SetMovement(Vector2.zero);
                return;
            }

            PerformMovementTowardsTarget(Target);
        }

        public float DistanceToIgnoreFriendlyFire => 1f;

        protected bool ShouldFollowTarget()
        {
            return followTarget;
        }

        protected AttackContext CreateAttackContext()
        {
            return new AttackContext
            {
                Direction = (Target.transform.position - Creature.transform.position).normalized,
                Target = Target,
                Attacker = Creature,
                TargetPosition = Target.transform.position,
                Team = Creature.Team,
            };
        }

        protected void PerformAttack(AttackContext context)
        {
            Creature.Weapon.ContinuousAttack(context);
        }

        private void TrackMovement()
        {
            var currentPosition = (Vector2)Creature.transform.position;
            var currentDirection = Creature.Movement.MoveDirection;

            bool movedByPosition = Vector2.Distance(currentPosition, _lastPosition) > 0.01f;

            bool rotatedDirection = false;
            if (currentDirection.sqrMagnitude > 0.0001f && _lastDirection.sqrMagnitude > 0.0001f)
            {
                // dot product is cosine of angle between normalized vectors
                float dot = Vector2.Dot(currentDirection.normalized, _lastDirection.normalized);
                // cos(1°) ≈ 0.99985 → anything less means >1° turn
                rotatedDirection = dot < 0.9998f;
            }
            else if (currentDirection.sqrMagnitude > 0.0001f && _lastDirection.sqrMagnitude <= 0.0001f)
            {
                // transition from idle → moving counts as movement
                rotatedDirection = true;
            }

            if (movedByPosition || rotatedDirection)
            {
                _lastMovedTime = Time.time;
                _lastDirection = currentDirection;
                _lastPosition = currentPosition;
            }
        }

        private void ResetMoveIfStuck()
        {
            if (_moveCommandTarget.HasValue && Time.time - _lastMovedTime > moveTimeout)
            {
                _moveCommandTarget = null;
                Creature.SetMovement(Vector2.zero);
                InvokePathChanged(Enumerable.Empty<Vector2>());
            }
        }
    }
}