using Building;
using GameplayScene.Managers;
using Managers;
using UnityEngine;
using Zenject;

namespace CreatureControllers
{
    [RequireComponent(typeof(BuildingView))]
    public class TurretController : AiController
    {
        [Inject] private IEntityManager _entityManager;
        [Inject] private ITeamManager _teamManager;
        [Inject] private IEntityFinder _entityFinder;

        [SerializeField] private Teams enemyTeam;

        protected BuildingView BuildingView { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            BuildingView = GetComponent<BuildingView>();
        }

        protected override void Think()
        {
            base.Think();

            var closestEnemy = _entityFinder.GetClosestEntity(transform.position, enemyTeam);
            
            var attackCtx = new AttackContext
            {
                Attacker = BuildingView,
                Target = closestEnemy,
                Direction = closestEnemy
                    ? (closestEnemy.transform.position - BuildingView.transform.position).normalized
                    : Vector2.zero,
                TargetPosition = closestEnemy
                    ? closestEnemy.transform.position
                    : BuildingView.transform.position
            };

            if (BuildingView.Weapon.IsInRange(attackCtx))
            {
                BuildingView.Weapon.ContinuousAttack(attackCtx);
            }
        }
    }
}