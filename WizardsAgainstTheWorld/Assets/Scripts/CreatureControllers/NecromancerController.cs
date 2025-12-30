using CreatureControllers;
using Items.Weapons;
using UnityEngine;

public class NecromancerController : AiController
{
    private Creature _target;

    [SerializeField] private bool moveOnAttackCooldown = false;
    [SerializeField] private float wanderingRange;

    private Vector2 _centerOfInterest;
    private Vector2 _goal;

    [SerializeField] private ProjectileWeapon projectileWeapon;
    [SerializeField] private TouchWeapon touchWeapon;


    protected override void Awake()
    {
        base.Awake();
        _centerOfInterest = transform.position;
        _goal = GetRandomPosition(_centerOfInterest, wanderingRange);
    }


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
     
        RangeAttack();
        
        TouchAttack();
        
        Movement();
    }
    

    private void Movement()
    {
        if (Vector2.Distance(Creature.transform.position, _goal) < 0.5f)
        {
            _goal = GetRandomPosition(_centerOfInterest, wanderingRange);
        }

        Debug.DrawLine(Creature.transform.position, _goal, Color.yellow);
        PerformMovementTowardsPosition(_goal);
    }
    
    private void RangeAttack()
    {
        var attackContext = new AttackContext()
        {
            Direction = (_target.transform.position - Creature.transform.position).normalized,
            Target = _target,
            Attacker = Creature,
            TargetPosition = _target.transform.position
        };


        if (projectileWeapon.GetOnCooldown(attackContext) && !moveOnAttackCooldown)
            return;


        if (IsInRange(_target, projectileWeapon.Range) &&
            PathClear(_target, 0.5f)) // TODO: Magic number, its the radius of the creature of a size of a human
        {
            PerformAttack(projectileWeapon, attackContext);
            return;
        }
    }
    
    private void TouchAttack()
    {
        var attackContext = new AttackContext()
        {
            Direction = (_target.transform.position - Creature.transform.position).normalized,
            Target = _target,
            Attacker = Creature,
            TargetPosition = _target.transform.position
        };
        
        
        if (touchWeapon.GetOnCooldown(attackContext) && !moveOnAttackCooldown)
            return;

        if (IsInRange(_target, touchWeapon.Range))
        {
            PerformAttack(touchWeapon, attackContext);
            return;
        }
    }

    private void PerformAttack(Weapon weapon, AttackContext ctx)
    {
        weapon.ContinuousAttack(ctx);
    }

    private Vector2 GetRandomPosition(Vector2 center, float radius)
    {
        return new Vector2(center.x + Random.Range(-radius / 2f, radius / 2), center.y + Random.Range(-radius / 2f, radius / 2));
    }
}