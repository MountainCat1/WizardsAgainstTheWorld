using System;
using UnityEngine;

public class SimpleCreatureAnimator : CreatureAnimator
{
    [Header("Animation Clips to Override")] [SerializeField]
    private AnimationClip idle;

    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip attack;

    private Weapon _weapon;

    /// <summary>
    /// In Awake, we load the base controller from Resources and override its clips.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // 1. Load a base controller named "MyBaseCreatureController.controller" from a Resources folder
        var baseController = Resources.Load<RuntimeAnimatorController>("BaseCreatureAnimator");
        if (baseController == null)
        {
            GameLogger.LogError("Could not find 'BaseCreatureAnimator.controller' in a Resources folder!");
            return;
        }

        // 2. Create an AnimatorOverrideController from that base
        AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);

        // 3. Override "Idle", "Walk", and "Run" states with your custom clips
        overrideController["Idle"] = idle;
        overrideController["Walk"] = walk;
        overrideController["Attack"] = attack;

        // 4. Assign the override controller to the Animator
        _anim.runtimeAnimatorController = overrideController;


        if (Creature.Weapon != null)
        {
            Creature.Weapon.Stroked += OnWeaponStroked;
        }

        Creature.WeaponChanged += OnWeaponChanged;
    }

    private void OnDestroy()
    {
        if (Creature != null)
        {
            Creature.StateChanged -= OnState;
            Creature.WeaponAttacked -= OnAttack;
            Creature.WeaponChanged -= OnWeaponChanged;
        }

        if (_weapon != null)
            _weapon.Stroked -= OnWeaponStroked;
    }

    private void OnWeaponChanged()
    {
        if (Creature.Weapon != null)
        {
            Creature.Weapon.Stroked += OnWeaponStroked;
            _weapon = Creature.Weapon;
        }
        else
        {
            GameLogger.LogWarning($"Creature {Creature.name} has no weapon assigned!");
        }
    }

    private void OnWeaponStroked()
    {
        _anim.Play("Attack", 0, 0f);
    }

    protected override void Start()
    {
        base.Start();

        if (Creature != null)
        {
            Creature.StateChanged += OnState;
            Creature.WeaponAttacked += OnAttack;
        }
        else
        {
            GameLogger.LogWarning($"No Creature assigned to {name}!");
        }
    }

    private void OnAttack(AttackContext ctx)
    {
        if (ctx.Direction.x != 0)
            _spriteRenderer.flipX = ctx.Direction.x > 0;
    }


    /// <summary>
    ///  OnMove is called whenever the creature moves (and is hooked to Creature.Moved).
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Idle:
                _anim.Play("Idle");
                break;
            case CreatureState.Moving:
                _anim.Play("Walk");
                break;
            case CreatureState.Attacking:
                _anim.Play("Attack");
                break;
            default:
                throw new NotImplementedException();
        }
    }
}