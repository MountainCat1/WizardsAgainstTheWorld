using System;
using CreatureControllers;
using UnityEngine;

public class MeleeJumpingEnemyController : AiController
{
    private Creature _target;

    [SerializeField] private bool moveOnAttackCooldown = false;

    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCooldown = 4f;
    [SerializeField] private float jumpRange = 5f;

    private DateTime _lastJumpedTime;

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
            TargetPosition = _target.transform.position
        };

        if (Creature.Weapon.GetOnCooldown(attackContext) && !moveOnAttackCooldown)
            return;

        if(Vector2.Distance(Creature.transform.position, _target.transform.position) < jumpRange && DateTime.Now - _lastJumpedTime > TimeSpan.FromSeconds(jumpCooldown))
        {
            _lastJumpedTime = DateTime.Now;
            var direction = (_target.transform.position - Creature.transform.position).normalized;
            Creature.Push(direction * jumpForce);
            return;
        }
        
        
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