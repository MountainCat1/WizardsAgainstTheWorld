using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Explosion : MonoBehaviour, IFreeable
    {
        private bool _used = false;
        private Action _free;

        public void Deinitialize()
        {
            _used = false;
        }   

        public void Initialize(Action free)
        {
            if (_used)
            {
                GameLogger.LogError("Explosion already initialized!");
            }
            
            _used = true;
           _free = free; 
        }
    }
}