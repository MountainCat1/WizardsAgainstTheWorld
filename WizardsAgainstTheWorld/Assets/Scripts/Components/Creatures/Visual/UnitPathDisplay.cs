using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UnitPathDisplay : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    [SerializeField] private UnitController unitController;
    [SerializeField] private GameObject goalDisplay;
    
    private Seeker _seeker;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _seeker = unitController.GetComponent<Seeker>();

        unitController.PathChanged += (path) => OnPathCalculated(path.ToArray());
    }

    private void OnPathCalculated(ICollection<Vector3> path)
    {
        _lineRenderer.positionCount = 0;

        if (path == null || path.Count <= 1)
        {
            goalDisplay.SetActive(false);
            return;
        }

        foreach (var point in path.Reverse())
        {
            _lineRenderer.SetPosition(_lineRenderer.positionCount++, point);
        }
        
        goalDisplay.transform.position = path.Last();
        goalDisplay.SetActive(true);
    }
}