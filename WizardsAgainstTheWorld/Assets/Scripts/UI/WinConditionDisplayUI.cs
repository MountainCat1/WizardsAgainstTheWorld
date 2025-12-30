using Managers;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class WinConditionDisplayUI : MonoBehaviour
    {
        [Inject] private IVictoryConditionManager _victoryConditionManager;
        
        [SerializeField] private TextMeshProUGUI winConditionText;

        private void Awake()
        {
            _victoryConditionManager.VictoryConditionsChanged += OnWinConditionChanged;
        }

        public void OnWinConditionChanged()
        {
            var winConditions = _victoryConditionManager.Conditions;
            var text = "";
            foreach (var (condition, value) in winConditions)
            {
                if(value)
                    text += $"<s>{condition.GetDescription()}</s>\n";
                else
                    text += $"{condition.GetDescription()}\n";
            }

            winConditionText.text = text;
        }
    }
}