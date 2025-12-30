using System;
using UnityEngine;

namespace Managers
{
    public interface IVisionManager
    {
        float VisionRangeMultiplier { get; set; }
        event Action Changed;
    }
    
    public class VisionManager : MonoBehaviour, IVisionManager
    {
        [SerializeField] private float visionRangeMultiplier;

        public float VisionRangeMultiplier
        {
            get => visionRangeMultiplier;
            set
            {
                visionRangeMultiplier = value;
                Changed?.Invoke(); // Fire event
            }
        }

        public event Action Changed;
    }
}