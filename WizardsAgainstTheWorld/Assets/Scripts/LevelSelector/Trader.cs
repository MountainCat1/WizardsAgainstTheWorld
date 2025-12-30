using ScriptableObjects;
using UnityEngine;

namespace LevelSelector
{
    [CreateAssetMenu(fileName = "Trader", menuName = "Custom/Trader")]
    public class Trader : ScriptableObject
    {
        public string NameKey => $"UI.Trader.{traderName}.Name";
        public string DescriptionKey => $"UI.Trader.{traderName}.Description";
        
        [SerializeField] public string traderName;
        [SerializeField] [TextArea] public string traderDescription;
        [SerializeField] public Sprite traderIcon;
        [SerializeField] public int maxItems = 5;
        [SerializeField] public int minItems = 1;
        [SerializeField] public LootTable traderItems;

        public string GetIdentifier()
        {
            return name;
        }
    }
}