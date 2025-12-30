using UnityEngine;

namespace ScriptableActions
{
    public class KillCreatureAction : ScriptableAction
    {
        [SerializeField] private Creature _targetCreature;

        public override void Execute()
        {
            base.Execute();

            if (_targetCreature == null)
            {
                Debug.LogWarning("Target creature is not set for KillCreatureAction.");
                return;
            }

            _targetCreature.Health.Damage(new HitContext()
            {
                Target = _targetCreature,
                Damage = _targetCreature.Health.CurrentValue,
            });
        }
    }
}