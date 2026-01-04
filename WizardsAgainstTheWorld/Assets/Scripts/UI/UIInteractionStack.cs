using System;
using System.Collections.Generic;
using GameplayScene.UI;
using UnityEngine;
using Zenject;

namespace UI
{
    public interface IUIInteraction
    {
        /// Called when this interaction becomes active
        void Enter();

        /// Called when cancelled (Escape or overridden)
        void Exit();

        /// Optional: block lower layers input
        bool BlocksInput { get; }
    }

    public interface IUIInteractionStack
    {
        void Push(IUIInteraction interaction);
        void CancelTop();
        bool IsBlocked();
        void Remove(EntityInspectorUI entityInspectorUI);
    }
    public sealed class UIInteractionStack : MonoBehaviour, IUIInteractionStack
    {
        public static UIInteractionStack Instance { get; private set; }

        private readonly Stack<IUIInteraction> _stack = new();
        
        [Inject] private IInputManager _inputManager;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _inputManager.UI.GoBack += CancelTop;
        }

        public void Push(IUIInteraction interaction)
        {
            _stack.Push(interaction);
            interaction.Enter();
        }

        public void CancelTop()
        {
            if (_stack.Count == 0)
                return;

            var interaction = _stack.Pop();
            interaction.Exit();
        }

        public bool IsBlocked()
        {
            foreach (var interaction in _stack)
            {
                if (interaction.BlocksInput)
                    return true;
            }

            return false;
        }

        public void Remove(EntityInspectorUI entityInspectorUI)
        {
            var tempStack = new Stack<IUIInteraction>();
            while (_stack.Count > 0)
            {
                var top = _stack.Pop();
                if (top == entityInspectorUI)
                    break;
                tempStack.Push(top);
            }

            while (tempStack.Count > 0)
            {
                _stack.Push(tempStack.Pop());
            }
        }
    }
}