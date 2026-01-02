using UnityEngine;

namespace CreatureControllers
{
    public class HarasserUnitController : UnitController
    {
        private Vector2? _retreatDirection;
        private float _retreatDelayTimer = 0f;
        
        [SerializeField] private float retreatDelayDuration = 0.3f; // Delay in seconds after attack before fleeing

        protected override void Think()
        {
            // Check if target is gone mid-retreat
            if (Target == null || !EntityManager.IsAliveAndActive(Target))
            {
                Target = null;
                _retreatDirection = null;
                _retreatDelayTimer = 0f;
                Creature.SetMovement(Vector2.zero);
            }

            // Handle post-attack retreat delay
            if (_retreatDelayTimer > 0f)
            {
                _retreatDelayTimer -= Time.deltaTime;
                Creature.SetMovement(Vector2.zero);
                return;
            }

            // If currently retreating
            if (_retreatDirection.HasValue)
            {
                var attackContext = CreateAttackContext();
                if (!Creature.Weapon.GetOnCooldown(attackContext))
                {
                    _retreatDirection = null;
                    Creature.SetMovement(Vector2.zero);
                }
                else
                {
                    Creature.SetMovement(_retreatDirection.Value.normalized);
                }
                return;
            }

            if (InteractionTarget != null)
            {
                return; // Harassers don't interact
            }

            if (Creature.Weapon is null)
            {
                Creature.SetMovement(Vector2.zero);
                return;
            }

            if (Target == null || !EntityManager.IsAliveAndActive(Target))
            {
                Target = GetNewTarget();
            }

            if (Target != null)
            {
                HandleHarassAttack();
                return;
            }

            Creature.SetMovement(Vector2.zero);
        }

        private void HandleHarassAttack()
        {
            var attackContext = CreateAttackContext();

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

            float distance = Vector2.Distance(Creature.transform.position, Target.transform.position);
            if (distance < Creature.Weapon.Range)
            {
                if (!Creature.Weapon.GetOnCooldown(attackContext))
                {
                    PerformAttack(attackContext);

                    if (Target == null || !Target.gameObject.activeInHierarchy || !EntityManager.IsAliveAndActive(Target))
                    {
                        // Don't retreat if target is now dead
                        _retreatDirection = null;
                        return;
                    }

                    _retreatDirection = (Creature.transform.position - Target.transform.position).normalized;
                    _retreatDelayTimer = retreatDelayDuration;
                }
                else
                {
                    _retreatDirection = (Creature.transform.position - Target.transform.position).normalized;
                    Creature.SetMovement(_retreatDirection.Value.normalized);
                }
            }
            else
            {
                PerformMovementTowardsTarget(Target);
            }
        }
    }
}
