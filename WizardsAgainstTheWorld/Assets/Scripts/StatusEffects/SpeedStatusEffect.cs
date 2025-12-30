using UnityEngine;

namespace Managers
{
    public class SpeedStatusEffect : StatusEffect
    {
        [field: SerializeField] private float SpeedModifier { get; set; } = 0f;
        
        protected override void StartStatusEffect()
        {
         // k   Target.Movement.Speed.AddModifier(this, SpeedModifier);
        }

        protected override void OnEndStatusEffect()
        {
            base.OnEndStatusEffect();
        }
    }
}