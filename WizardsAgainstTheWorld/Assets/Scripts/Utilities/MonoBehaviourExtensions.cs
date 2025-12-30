using System.Collections;
using UnityEngine;

namespace Utilities
{
    public enum RandomOffsetType
    {
        Small,  // nearly physics
        Medium, // regeneration, seeing things
        Large   // checking if something happened before
    }

    public static class MonoBehaviourExtensions
    {
        public static Coroutine StartRandomOffsetCoroutine(
            this MonoBehaviour monoBehaviour,
            IEnumerator coroutine,
            RandomOffsetType randomOffsetType = RandomOffsetType.Small)
        {
            float offset = GetOffset(randomOffsetType);
            return monoBehaviour.StartCoroutine(WrapWithOffset(coroutine, offset));
        }

        private static float GetOffset(RandomOffsetType type)
        {
            switch (type)
            {
                case RandomOffsetType.Small:
                    return Random.Range(0.1f, 0.5f);
                case RandomOffsetType.Medium:
                    return Random.Range(0.5f, 1.5f);
                case RandomOffsetType.Large:
                    return Random.Range(1.5f, 3f);
                default:
                    return 0f;
            }
        }

        private static IEnumerator WrapWithOffset(IEnumerator coroutine, float delay)
        {
            yield return new WaitForSeconds(delay);
            yield return coroutine;
        }
    }
}