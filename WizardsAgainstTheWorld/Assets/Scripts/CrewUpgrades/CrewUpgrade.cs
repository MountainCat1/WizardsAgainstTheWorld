using Data;
using UnityEngine;
using Zenject;

namespace CrewUpgrades
{
    public class CrewUpgrade : MonoBehaviour
    {
        public string GetIdentifier()
        {
            return Name;
        }

        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        
        public string NameKey => $"Items.{Name}.Name";
        public string DescriptionKey => $"Items.{Name}.Description";
        
        public CrewUpgradeData ToData()
        {
            var data = new CrewUpgradeData
            {
                Id = GetIdentifier()
            };
            return data;
        }
        
        public virtual void InitializeGameScene(DiContainer diContainer)
        {
            // This method can be overridden in derived classes to perform initialization
            // when the game scene is loaded.
        }
        
        public  virtual void InitializeLevelSelectorScene(DiContainer diContainer)
        {
            // This method can be overridden in derived classes to perform initialization
            // when the level selector scene is loaded.
        }
    }
}