using UnityEngine;

namespace ScriptableObjects
{
    public interface IGameConfiguration
    {
        public float GameTime { get; }
    
        // UI
        public float TypingSpeed { get; }
        public float TypingDelay => 1f / TypingSpeed;
        public float FastTypingSpeed { get; }
        public float FastTypingDelay => 1f / FastTypingSpeed;
    }

    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "GameConfiguration")]
    public class GameConfiguration : ScriptableObject, IGameConfiguration
    {
        [field: SerializeField] public float GameTime { get; private set; }
        [field: SerializeField] public float TypingSpeed { get; private set; }
        [field: SerializeField] public float FastTypingSpeed { get; private set; }
    }
}