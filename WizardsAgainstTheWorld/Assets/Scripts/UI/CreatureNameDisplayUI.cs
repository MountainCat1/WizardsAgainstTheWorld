using System.Linq;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CreatureNameDisplayUI : MonoBehaviour
    {
        [SerializeField] private Creature creature;
        
        private TextMeshProUGUI _textMeshProUGUI;
        
        private void Awake()
        {
            _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (creature == null)
            {
                GameLogger.LogError("Host creature is not assigned in the inspector.");
                return;
            }

            _textMeshProUGUI.text = creature.name.Split(' ').First();
        }
    }
}