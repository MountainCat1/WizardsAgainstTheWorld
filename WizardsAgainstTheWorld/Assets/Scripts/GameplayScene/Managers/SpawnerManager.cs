using System;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ISpawnerManager
    {
        public event Action<Component> Spawned;

        T Spawn<T>(T prefab, Vector2 position) where T : Component;
        T Spawn<T>(T prefab, Vector2 position, Transform parent) where T : Component;
        T Spawn<T>(T prefab, Transform parent) where T : Component;
        GameObject Spawn(GameObject prefab, Vector2 position);
    }

    public class SpawnerManager : MonoBehaviour, ISpawnerManager
    {
        public event Action<Component> Spawned;

        [Inject] DiContainer _container;
        
        public T Spawn<T>(T prefab, Vector2 position) where T : Component
        {
            var spawnedObject = _container.InstantiatePrefab(prefab, position, Quaternion.identity, null)
                .GetComponent<T>();

            Spawned?.Invoke(spawnedObject);

            return spawnedObject;
        }

        public T Spawn<T>(T prefab, Vector2 position, Transform parent) where T : Component
        {
            var spawnedObject = _container.InstantiatePrefab(prefab, position, Quaternion.identity, parent)
                .GetComponent<T>();

            Spawned?.Invoke(spawnedObject);

            return spawnedObject;
        }

        public T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            return Spawn(
                prefab: prefab,
                position: parent.position,
                parent: parent
            );
        }

        public GameObject Spawn(GameObject prefab, Vector2 position)
        {
            return _container.InstantiatePrefab(prefab, position, Quaternion.identity, null);
        }
    }
}