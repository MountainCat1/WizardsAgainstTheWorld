using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class DynamicSoundtrackPlayer : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> actionSoundtracks;
        [SerializeField] private int enemiesToTriggerActionSoundtrack = 50;
        
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private ISoundManager _soundManager;

        private void Start()
        {
            StartCoroutine(CheckForActionCoroutine());
        }

        private IEnumerator CheckForActionCoroutine()
        {
            while (true)
            {
                var memorizedEnemies = _creatureManager.PlayerCreatures
                    .Select(x => x.Controller)
                    .Where(x => x.GetType() == typeof(UnitController))
                    .Cast<UnitController>()
                    .SelectMany(x => x.GetMemorizedCreatures())
                    .Distinct()
                    .Sum(x => x.XpAmount);
                

                if (memorizedEnemies > enemiesToTriggerActionSoundtrack)
                    break;
                
                yield return new WaitForSeconds(1f);
            }
         
            _soundManager.SetSoundtrack(actionSoundtracks);
        }
    }
}