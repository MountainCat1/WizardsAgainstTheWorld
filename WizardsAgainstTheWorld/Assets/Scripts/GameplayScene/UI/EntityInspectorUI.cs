using TMPro;
using UnityEngine;

namespace GameplayScene.UI
{
    public class EntityInspectorUI : MonoBehaviour
    {
        public Entity InspectedEntity { get; private set; }
        
        [SerializeField] private TMP_Text entityNameText;
        
        public void Initialize(Entity entity)
        {
            entityNameText.text = entity.name;
            
            InspectedEntity = entity;
        }

        public void Deinitialize()
        {
            InspectedEntity = null;
        }
    }
}