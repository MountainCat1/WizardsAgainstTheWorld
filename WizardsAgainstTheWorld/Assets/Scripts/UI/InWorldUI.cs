using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace UI
{
    
    /// <summary>
    /// This script is used for UI elements which are supposed to be hidden when not visible from the perspective of player units.
    /// </summary>
    public class InWorldUI : MonoBehaviour
    {
        private const float UpdateInterval = 1f;
        
        [Inject] private ITeamManager _teamManager;
        [Inject] private ICreatureManager _creatureManager;
        
        [SerializeField] private Creature hostCreature;
        [FormerlySerializedAs("toggableUI")] [SerializeField] private GameObject togglableUI;

        private void Start()
        {
            if (hostCreature == null)
            {
                GameLogger.LogError("Host creature is not assigned in the inspector.");
                return;
            }

            if (togglableUI == null)
            {
                GameLogger.LogError("TogglableUI is not assigned in the inspector.");
                return;
            }

            togglableUI.SetActive(false);
            
            // Start the coroutine to update the UI visibility
            float randomOffset = UnityEngine.Random.Range(0f, UpdateInterval);
            InvokeRepeating(nameof(SlowUpdateCoroutine), randomOffset, UpdateInterval);
        }

        private void SlowUpdateCoroutine()
        {
            // TODO: optimize this
            // TODO: FOR REAL OPTIMIZE THIS SHIT
            // 24% _creatureManager.GetCreaturesAliveActive
            // 7.8% Enumerable.ToList
            // 0.6% Enumerable.Any
            var playerUnits = _creatureManager.GetCreaturesAliveActive().Where(x => x.Team == Teams.Player);
            
            if(playerUnits.Any(x => x.Controller.CanSee(hostCreature)))
            {
                togglableUI.SetActive(true);
            }
            else
            {
                togglableUI.SetActive(false);
            }
        }
    }
}