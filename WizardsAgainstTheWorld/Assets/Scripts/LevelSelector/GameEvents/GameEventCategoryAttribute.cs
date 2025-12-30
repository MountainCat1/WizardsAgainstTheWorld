using System;

namespace LevelSelector.GameEvents
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GameEventCategoryAttribute : Attribute
    {
        public GameEventCategory Category { get; }

        public GameEventCategoryAttribute(GameEventCategory category)
        {
            Category = category;
        }
    }

    public enum GameEventCategory
    {
        Default,
        System,
        Misc,
    }
}