using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using Zenject;

public interface ICyclingPoolingManager : IPoolManager
{
    IPoolAccess<T> GetOrCreatePool<T>(string poolName, int cap) where T : Component;
}

public class CyclingPoolingManager : MonoBehaviour, ICyclingPoolingManager
{
    [Inject] private ISpawnerManager _spawnerManager = null!;
    [SerializeField] private int defaultPoolCap = 30;

    private readonly Dictionary<string, ICyclingObjectPool> _pools = new();

    public T SpawnObject<T>(T prefab, Vector2 position, float rotation = 0, Transform parent = null) where T : Component
    {
        return SpawnObject(prefab, prefab.GetType().FullName!, position, rotation, parent);
    }

    public T SpawnObject<T>(T prefab, string poolName, Vector2 position, float rotation = 0, Transform parent = null) where T : Component
    {
        var pool = GetOrCreateTypedPool<T>(poolName, defaultPoolCap);
        return pool.Spawn(prefab, position, rotation, parent);
    }

    public void DespawnObject<T>(T node) where T : Component => DespawnObject(node, typeof(T).FullName!);

    public void DespawnObject<T>(T node, string poolName) where T : Component
    {
        if (_pools.TryGetValue(poolName, out var pool) && pool is CyclingObjectPool<T> typed)
            typed.ForceDespawn(node);
    }

    public IReadOnlyCollection<T> GetInUseObjects<T>() where T : Component => GetInUseObjects<T>(typeof(T).FullName!);

    public IReadOnlyCollection<T> GetInUseObjects<T>(string poolName) where T : Component
    {
        var pool = GetOrCreateTypedPool<T>(poolName, defaultPoolCap);
        return pool.GetInUseObjects();
    }

    public IPoolAccess<T> CreatePool<T>() where T : Component
    {
        var name = Guid.NewGuid().ToString();
        GetOrCreateTypedPool<T>(name, defaultPoolCap);
        return new PoolAccess<T>(this, name);
    }

    public IPoolAccess<T> GetPoolAccess<T>(string poolName) where T : Component
    {
        GetOrCreateTypedPool<T>(poolName, defaultPoolCap);
        return new PoolAccess<T>(this, poolName);
    }

    public IPoolAccess<T> GetOrCreatePool<T>(string poolName, int cap) where T : Component
    {
        GetOrCreateTypedPool<T>(poolName, cap);
        return new PoolAccess<T>(this, poolName);
    }

    private CyclingObjectPool<T> GetOrCreateTypedPool<T>(string poolName, int cap) where T : Component
    {
        if (_pools.TryGetValue(poolName, out var existing))
            return (CyclingObjectPool<T>)existing;

        var pool = new CyclingObjectPool<T>(_spawnerManager, cap);
        _pools[poolName] = pool;
        return pool;
    }

    private interface ICyclingObjectPool { }

    private class CyclingObjectPool<T> : ICyclingObjectPool where T : Component
    {
        private readonly ISpawnerManager _spawner;
        private readonly Queue<T> _inUse = new();
        private readonly List<T> _allObjects = new();
        private readonly int _cap;

        public CyclingObjectPool(ISpawnerManager spawner, int cap)
        {
            _spawner = spawner;
            _cap = cap;
        }

        public T Spawn(T prefab, Vector2 position, float rotation, Transform parent)
        {
            T obj;

#if UNITY_EDITOR
            if (!_allObjects.Any())
            {
                GameLogger.Log($"Cycling pool for {typeof(T)} is empty. Spawning a new object. {prefab.name}");
            }
#endif
            
            if (_allObjects.Count < _cap)
            {
                obj = _spawner.Spawn<T>(prefab, position, parent)
                      ?? throw new InvalidOperationException($"Failed to spawn object of type {typeof(T)}");
                _allObjects.Add(obj);
            }
            else
            {
                obj = _inUse.Dequeue();
                GameLogger.Log($"Cycling pool: Reusing {obj.name}");
            }

            obj.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, rotation));
            obj.gameObject.SetActive(true);
            _inUse.Enqueue(obj);

            if (obj is IFreeable freeable)
                freeable.Initialize(() => ForceDespawn(obj));

            return obj;
        }

        public void ForceDespawn(T obj)
        {
            if (!_inUse.Contains(obj)) return;

            var temp = new Queue<T>();
            while (_inUse.Count > 0)
            {
                var item = _inUse.Dequeue();
                if (!EqualityComparer<T>.Default.Equals(item, obj))
                    temp.Enqueue(item);
            }

            _inUse.Clear();
            foreach (var t in temp)
                _inUse.Enqueue(t);

            obj.gameObject.SetActive(false);
            if (obj is IFreeable freeable)
                freeable.Deinitialize();
        }

        public IReadOnlyCollection<T> GetInUseObjects() => _inUse.ToList();
    }
}
