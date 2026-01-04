using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameplayScene.UI
{
    public class EntityInspectorManagerUI : MonoBehaviour
    {
        [Inject] private IInputMapper _inputMapper;

        [SerializeField] private EntityInspectorUI entityInspectorUI;

        private void Start()
        {
            _inputMapper.OnEntityClicked += OnEntityClicked;
            
            entityInspectorUI.Deinitialize();
        }

        private void OnEntityClicked(Entity entity)
        {
            if (entityInspectorUI.InspectedEntity == entity)
            {
                entityInspectorUI.Deinitialize();
                entityInspectorUI.gameObject.SetActive(false);
                return;
            }

            entityInspectorUI.gameObject.SetActive(true);
            entityInspectorUI.Initialize(entity);
        }
    }
}