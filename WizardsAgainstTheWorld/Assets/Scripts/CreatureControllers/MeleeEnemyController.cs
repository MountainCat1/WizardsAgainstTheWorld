using CreatureControllers;
using UnityEngine;

public class MeleeEnemyController : AiController
{
    private Creature _target;

    [SerializeField] private bool moveOnAttackCooldown = false;

    private void Update()
    {
        Creature.SetMovement(Vector2.zero);
        
        if (!_target)
        {
            _target = GetNewTarget();

            if (!_target)
            {
                return;
            }
        }

        var attackContext = new AttackContext()
        {
            Direction = (_target.transform.position - Creature.transform.position).normalized,
            Target = _target,
            Attacker = Creature,
            TargetPosition = _target.transform.position,
        };
                
        if(Creature.Weapon.GetOnCooldown(attackContext) && !moveOnAttackCooldown)
            return;

        if (Vector2.Distance(Creature.transform.position, _target.transform.position) < Creature.Weapon.Range)
        {
            PerformAttack(attackContext);
            return;
        }

        PerformMovementTowardsTarget(_target);
    }

    private void PerformAttack(AttackContext ctx)
    {
        Creature.Weapon.ContinuousAttack(ctx);
    }
}