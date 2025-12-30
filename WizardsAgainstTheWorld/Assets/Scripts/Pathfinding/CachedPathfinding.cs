using System;
using System.Collections.Generic;
using UnityEngine;

public class CachedPathfinding : IPathfinding
{
    private readonly GridGenerator _gridGenerator;
    private readonly OldPathfinding _innerPathfinding;

    private readonly Dictionary<(Node, Node), List<Node>> _pathCache = new();
    private readonly Dictionary<(Vector2, Vector2), bool> _clearPathCache = new();
    private readonly Dictionary<Vector2, bool> _walkableCache = new();
    private readonly Dictionary<(Vector2, int), ICollection<Vector2>> _spreadPositionCache = new();

    public CachedPathfinding(OldPathfinding innerPathfinding, GridGenerator gridGenerator)
    {
        _innerPathfinding = innerPathfinding;
        _gridGenerator = gridGenerator;
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = _gridGenerator.NodeFromWorldPoint(startPos);
        Node targetNode = _gridGenerator.NodeFromWorldPoint(targetPos);

        var key = (startNode, targetNode);
        if (_pathCache.TryGetValue(key, out var cachedPath))
        {
            return cachedPath;
        }

        GameLogger.Log($"Path not found in cache, calculating path from {startNode.worldPosition} to {targetNode.worldPosition}");
        var path = _innerPathfinding.FindPath(startPos, targetPos);
        _pathCache[key] = path;
        return path;
    }

    public bool IsClearPath(Vector2 a, Vector2 b)
    {
        var key = (RoundToGrid(a), RoundToGrid(b));
        if (_clearPathCache.TryGetValue(key, out var cachedResult))
        {
            return cachedResult;
        }

        var result = _innerPathfinding.IsClearPath(a, b);
        _clearPathCache[key] = result;
        return result;
    }

    public bool IsWalkable(Vector2 targetPosition)
    {
        if (_walkableCache.TryGetValue(targetPosition, out var cachedResult))
        {
            return cachedResult;
        }

        var result = _innerPathfinding.IsWalkable(targetPosition);
        _walkableCache[targetPosition] = result;
        return result;
    }

    public ICollection<Vector2> GetSpreadPosition(Vector2 position, int amount)
    {
        var key = (position, amount);
        if (_spreadPositionCache.TryGetValue(key, out var cachedResult))
        {
            return cachedResult;
        }

        var result = _innerPathfinding.GetSpreadPosition(position, amount);
        _spreadPositionCache[key] = result;
        return result;
    }

    public void AddObstacle(IObstacle obstacle)
    {
        _innerPathfinding.AddObstacle(obstacle);
        obstacle.Moved += () =>
        {
            GameLogger.Log("Obstacle moved - invalidating cache");
            InvalidateCache(obstacle.Position, obstacle.Radius);
        };
        ClearCache();
    }

    public void RemoveObstacle(IObstacle obstacle)
    {
        _innerPathfinding.RemoveObstacle(obstacle);
        ClearCache();
    }

    public void InvalidateCache(Vector2 position, float radius)
    {
        // Remove affected paths
        var keysToRemovePath = new List<(Node, Node)>();
        foreach (var key in _pathCache.Keys)
        {
            if (Vector2.Distance(key.Item1.worldPosition, position) <= radius ||
                Vector2.Distance(key.Item2.worldPosition, position) <= radius)
            {
                keysToRemovePath.Add(key);
            }
        }
        foreach (var key in keysToRemovePath)
        {
            _pathCache.Remove(key);
        }

        // Remove affected clear path results
        var keysToRemoveClearPath = new List<(Vector2, Vector2)>();
        foreach (var key in _clearPathCache.Keys)
        {
            if (Vector2.Distance(key.Item1, position) <= radius ||
                Vector2.Distance(key.Item2, position) <= radius)
            {
                keysToRemoveClearPath.Add(key);
            }
        }
        foreach (var key in keysToRemoveClearPath)
        {
            _clearPathCache.Remove(key);
        }

        // Remove affected walkable results
        var keysToRemoveWalkable = new List<Vector2>();
        foreach (var key in _walkableCache.Keys)
        {
            if (Vector2.Distance(key, position) <= radius)
            {
                keysToRemoveWalkable.Add(key);
            }
        }
        foreach (var key in keysToRemoveWalkable)
        {
            _walkableCache.Remove(key);
        }

        // Remove affected spread position results
        var keysToRemoveSpreadPosition = new List<(Vector2, int)>();
        foreach (var key in _spreadPositionCache.Keys)
        {
            if (Vector2.Distance(key.Item1, position) <= radius)
            {
                keysToRemoveSpreadPosition.Add(key);
            }
        }
        foreach (var key in keysToRemoveSpreadPosition)
        {
            _spreadPositionCache.Remove(key);
        }
    }

    private void ClearCache()
    {
        _pathCache.Clear();
        _clearPathCache.Clear();
        _walkableCache.Clear();
        _spreadPositionCache.Clear();
    }
    
    private Vector2 RoundToGrid(Vector2 position)
    {
        const int precision = 1;
        return new Vector2((float)Math.Round(position.x, precision), (float)Math.Round(position.y, precision));
    }
}
