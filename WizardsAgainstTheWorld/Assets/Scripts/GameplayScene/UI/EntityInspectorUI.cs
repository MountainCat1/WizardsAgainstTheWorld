using Building;
using Building.UI;
using TMPro;
using UI.Abstractions;
using UnityEngine;

namespace GameplayScene.UI
{
    public class EntityInspectorUI : PageUI
    {
        public Entity InspectedEntity { get; private set; }
        
        [SerializeField] private TMP_Text entityNameText;
        
        [SerializeField] private BuildingInspectorUI buildingInspectorUI;
        
        public void Initialize(Entity entity)
        {
            entityNameText.text = entity.Name;
            
            InspectedEntity = entity;
            
            buildingInspectorUI.Deinitialize();

            switch (entity)
            {
                case BuildingView building:
                    buildingInspectorUI.Initialize(building);
                    break;
            }
        }

        public void Deinitialize()
        {
            InspectedEntity = null;
        }
    }
}