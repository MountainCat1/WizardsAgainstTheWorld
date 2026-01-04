using Managers;
using UnityEngine;
using Zenject;

namespace UI
{
    public class VictoryScreenUI : MonoBehaviour
    {
        [Inject] private IVictoryConditionManager _victoryConditionManager;
        
        [SerializeField] private GameObject victoryScreen;
        
        private void Start()
        {
            _victoryConditionManager.EndGameAchieved += ShowVictoryScreen;
            victoryScreen.SetActive(false);
        }

        private void ShowVictoryScreen()
        {
            victoryScreen.SetActive(true);
        }
    }
}