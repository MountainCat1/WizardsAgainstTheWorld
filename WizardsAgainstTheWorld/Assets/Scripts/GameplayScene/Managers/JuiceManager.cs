using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Zenject;

namespace Managers
{
    public class JuiceUsePreContext
    {
        public decimal JuiceUsed { get; set; }
        public bool IsOverdrive { get; set; }
    }
    
    public interface IJuiceManager
    {
        public event Action JuiceChanged;
        public decimal Juice { get; }
        float ConsumptionRate { get; }
        public void Initialize(IList<Creature> creatures, decimal juice);
        
        ProcessorEvent<JuiceUsePreContext> JuiceUseProcessor { get; }
    }

    public class JuiceManager : MonoBehaviour, IJuiceManager
    {
        public event Action JuiceChanged;
        public ProcessorEvent<JuiceUsePreContext> JuiceUseProcessor { get; } = new();
        
        public decimal Juice { get; private set; }

        public float ConsumptionRate => consumerConsumptionRate * _creatures?.Count(x => x.gameObject.activeInHierarchy) ?? 0;

        [Inject] private ICreatureManager _creatureManager;
        [Inject] private IInputManager _inputManager;
        [Inject] ITimeManager _timeManager;

        [SerializeField] private float consumerConsumptionRate = 1;
        [SerializeField] private float damageRate = 0.25f;
        [SerializeField] private float timeScaleOnOverdrive = 0.35f;
        [SerializeField] private float juiceConsumptionOnOverdrive = 0.5f;
        

        private List<Creature> _creatures;

        private bool _slowedDown = false;
        
        private void Start()
        {
            _inputManager.ToggleJuiceOverdrive += OnToggleJuiceOverdrive;
        }

        private void OnToggleJuiceOverdrive()
        {
            _slowedDown = !_slowedDown;

            if (_slowedDown)
            {
                _timeManager.AddTimeScaleChange(TimeScaleModifier.JuiceOverdrive, timeScaleOnOverdrive);
            }
            else
            {
                _timeManager.RemoveTimeScaleChange(TimeScaleModifier.JuiceOverdrive);
            }
        }

        public void Initialize(IList<Creature> creatures, decimal juice)
        {
            Juice = juice;
            JuiceChanged?.Invoke();
            
            _creatures = new List<Creature>(creatures);

            foreach (var creature in creatures)
            {
                creature.Health.Death += OnCreatureDeath;
            }

            StartCoroutine(DamageRoutine());
        }

        private IEnumerator DamageRoutine()
        {
            while (true)
            {
                if (Juice > 0)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                foreach (var creature in _creatures.Where(x => x.enabled).ToArray())
                {
                    creature.Health.Damage(new HitContext()
                    {
                        Damage = damageRate,
                        Attacker = null,
                        PushFactor = 0,
                        Target = creature
                    });
                }
                
                yield return new WaitForSeconds(1);
            }
        }

        private void OnCreatureDeath(DeathContext ctx)
        {
            _creatures.Remove(ctx.KilledEntity as Creature);
        }

        private void Update()
        {
            var consumption = (decimal)(ConsumptionRate * Time.deltaTime);
            if(_slowedDown)
                consumption *= (decimal)juiceConsumptionOnOverdrive;
            
            var context = new JuiceUsePreContext
            {
                JuiceUsed = consumption,
                IsOverdrive = _slowedDown
            };
            
            JuiceUseProcessor.Invoke(context);
            
            Juice -= context.JuiceUsed;
            if(Juice < 0)
                Juice = 0;
            JuiceChanged?.Invoke();
        }
    }
}