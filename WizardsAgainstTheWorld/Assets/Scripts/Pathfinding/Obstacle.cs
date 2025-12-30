using System;
using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour, IObstacle
{
    [Inject] IPathfinding _pathfinding;


    // Implementing IObstacle
    Vector2 IObstacle.Position => transform.position;
    float IObstacle.Radius => radius;
    public event Action Moved;

    [SerializeField] private float radius;
    
    private const float MinMoveDistance = 0.05f;
    private Vector2 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void OnEnable()
    {
        _pathfinding.AddObstacle(this);
    }
    
    private void OnDisable()
    {
        _pathfinding.RemoveObstacle(this);
    }
    
    private void Update()
    {
        if (Vector2.Distance(transform.position, _lastPosition) > MinMoveDistance)
        {
            _lastPosition = transform.position;
            Moved?.Invoke();
        }
    }
    
}
