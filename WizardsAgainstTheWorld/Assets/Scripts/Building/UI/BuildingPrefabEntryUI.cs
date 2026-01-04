using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Building.UI
{
    [RequireComponent(typeof(Button))]
    public class BuildingPrefabEntryUI : MonoBehaviour
    {
        private BuildingPrefab _definition;
        
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;
        
        private Action<BuildingPrefab> _onSelectedCallback;
        
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);       
        }
        
        public void Initialize(BuildingPrefab definition, Action<BuildingPrefab> callback)
        {
            _definition = definition;
            
            iconImage.sprite = definition.GetComponentInChildren<SpriteRenderer>().sprite;
            nameText.text = definition.name;
            
            _onSelectedCallback = callback;
            
            costText.text = GetCostText(definition);
        }

        private string GetCostText(BuildingPrefab definition)
        {
            if (definition.costs == null || definition.costs.Count == 0)
                return "No Cost";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var cost in definition.costs)
            {
                sb.Append($"{cost.amount} {cost.type}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }

        private void OnButtonClicked()
        {
            Debug.Log($"Building '{_definition.name}' button clicked.");
            
            _onSelectedCallback.Invoke(_definition);
        }

    }
}