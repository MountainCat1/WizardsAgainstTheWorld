using System;
using UnityEngine;

namespace VictoryConditions
{
    [Serializable]
    public abstract class VictoryCondition : ScriptableObject
    {
        public abstract string GetDescription();
        public abstract bool Check();

        [SerializeField] private protected string descriptionKey;

        public string GetIdentifier()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("Victory condition name is not set.");
            }
            return name;
        }

        public virtual void Start()
        {
        }
    }
}