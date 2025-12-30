using System;
using System.Collections.Generic;
using System.IO;
using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Utilities
{
    public static class SaveLoadManager
    {
        private static Dictionary<Type, object> _saveables = new();

        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new CustomSerializationBinder(),
            Converters = new List<JsonConverter>
            {
                new Vector2Converter(),
                new Vector2IntConverter(),
                new StringEnumConverter()
            }
        };

        public static T Load<T>() where T : class, ISaveable<T>, new()
        {
            if (_saveables.GetValueOrDefault(typeof(T)) is T singleton)
            {
                return singleton;
            }

            var dummyInstance = new T();
            var path = Path.Combine(Application.persistentDataPath, dummyInstance.GetFileName());

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonConvert.DeserializeObject<T>(json, SerializerSettings);

                // Merge with defaults to fix missing properties
                var defaults = dummyInstance.CreateDefault();
                MergeWithDefaults(loaded, defaults);

                return loaded;
            }

            var created = dummyInstance.CreateDefault();
            Save(created);
            return created;
        }


        public static void Update<T>(ISaveable<T> saveable)
        {
            if (_saveables.ContainsKey(typeof(T)))
            {
                _saveables[typeof(T)] = saveable;
            }
            else
            {
                _saveables.Add(typeof(T), saveable);
            }
        }


        public static void Save<T>(ISaveable<T> saveable)
        {
            var path = Path.Combine(Application.persistentDataPath, saveable.GetFileName());
            var json = JsonConvert.SerializeObject(saveable, Formatting.Indented, SerializerSettings);
            File.WriteAllText(path, json);
            Update(saveable);
        }

        private static void MergeWithDefaults<T>(T target, T defaults)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var value = prop.GetValue(target);
                if (value == null)
                {
                    prop.SetValue(target, prop.GetValue(defaults));
                }
            }
        }
    }
}