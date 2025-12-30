using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LevelSelector.GameEvents
{
    public static class GameEventRegistry
    {
        private static bool _registered = false;

        // Stores each event along with its category (e.g. "System", "Misc")
        private static readonly Dictionary<string, (GameEvent Event, GameEventCategory Category)> Events = new();

        public static void RegisterAllFromType(Type type)
        {
            // Look for class-level category attribute
            var classCategoryAttr = type.GetCustomAttribute<GameEventCategoryAttribute>();
            var category = classCategoryAttr?.Category ?? GameEventCategory.Default;

            var gameEventFields = type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(GameEvent));

            foreach (var field in gameEventFields)
            {
                var gameEvent = (GameEvent)field.GetValue(null);
                if (gameEvent != null && !string.IsNullOrEmpty(gameEvent.Id))
                {
                    Events[gameEvent.Id] = (gameEvent, category);
                }
            }

            foreach (var registeredEvent in Events)
            {
                GameLogger.Log($"Registered game event: {registeredEvent.Key}");
            }
        }

        public static void RegisterAllFromAssembly(Assembly assembly)
        {
            var gameEventTypes = assembly
                .GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed); // static classes

            foreach (var type in gameEventTypes)
            {
                RegisterAllFromType(type);
            }
        }

        private static void EnsureRegistered()
        {
            if (!_registered)
            {
                RegisterAllFromAssembly(Assembly.GetExecutingAssembly());
                _registered = true;
            }
        }

        public static GameEvent Get(string id)
        {
            EnsureRegistered();
            Events.TryGetValue(id, out var result);
            return result.Event;
        }

        public static IEnumerable<GameEvent> GetAll()
        {
            EnsureRegistered();
            return GetAllExcludingCategory(GameEventCategory.System);
        }
        
        public static IEnumerable<GameEvent> GetByCategory(GameEventCategory category)
        {
            EnsureRegistered();
            return Events.Values
                .Where(e => Equals(e.Category, category))
                .Select(e => e.Event);
        }

        public static IEnumerable<GameEvent> GetAllExcludingCategory(GameEventCategory excludedCategory)
        {
            EnsureRegistered();
            return Events.Values
                .Where(e => !string.Equals(e.Category, excludedCategory))
                .Select(e => e.Event);
        }
    }
}
