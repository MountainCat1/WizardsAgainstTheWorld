using System.Net.NetworkInformation;
using UI;
using UnityEngine;
using Zenject;

namespace GameplayScene.UI
{
    public class EntityInspectorManagerUI : MonoBehaviour
    {
        [Inject] private IInputMapper _inputMapper;
        [Inject] private IUIInteractionStack _uiInteractionStack;

        [SerializeField] private EntityInspectorUI entityInspectorUI;

        private void Start()
        {
            _inputMapper.OnEntityClicked += OnEntityClicked;

            DeinitializeEntityInspector();
            entityInspectorUI.OnHide += OnInspectorHidden;
        }

        private void OnInspectorHidden()
        {
            DeinitializeEntityInspector();
        }

        private void OnEntityClicked(Entity entity)
        {
            if (entityInspectorUI.InspectedEntity == entity)
            {
                DeinitializeEntityInspector();
                _uiInteractionStack.Remove(entityInspectorUI);
                return;
            }

            if (entityInspectorUI.InspectedEntity == null)
            {
                _uiInteractionStack.Push(entityInspectorUI);
            }

            InitializeEntityInspector(entity);
        }
        
        private void InitializeEntityInspector(Entity entity)
        {
            entityInspectorUI.Initialize(entity);

            if (entity != null)
            {
                entity.Destroyed += DeinitializeEntityInspector;
            }
        }
        
        private void DeinitializeEntityInspector()
        {
            if (entityInspectorUI.InspectedEntity != null)
            {
                entityInspectorUI.InspectedEntity.Destroyed -= DeinitializeEntityInspector;
            }
            
            entityInspectorUI.Deinitialize();
        }
    }
}