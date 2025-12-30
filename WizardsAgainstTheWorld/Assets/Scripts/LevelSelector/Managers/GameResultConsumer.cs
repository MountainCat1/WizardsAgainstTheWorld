using System;
using Managers;
using UnityEngine;

namespace LevelSelector.Managers
{
    public interface IGameResultConsumer
    {
        event Action<GameResult> GameResultConsumed;
        void RegisterHandler(Action<GameResult> handler);
    }

    public class GameResultConsumer : MonoBehaviour, IGameResultConsumer
    {
        public event Action<GameResult> GameResultConsumed;

        private GameResult _gameResult;

        private void Awake()
        {
            _gameResult = GameManager.GameResult;
            GameManager.GameResult = null;
        }

        private void Start()
        {
            if (_gameResult != null)
            {
                GameResultConsumed?.Invoke(_gameResult);
            }
            else
            {
                GameLogger.Log("No GameResult available at start.");
            }
        }

        public void RegisterHandler(Action<GameResult> handler)
        {
            if (handler == null)
            {
                GameLogger.LogWarning("Cannot register a null handler for GameResult.");
                return;
            }

            if (_gameResult != null)
            {
                handler.Invoke(_gameResult);
            }

            GameResultConsumed += handler;
        }
    }
}