using System;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using Data;
using DefaultNamespace.Pathfinding;
using Managers;
using UnityEngine;
using Utilities;
using Zenject;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public event Action<ICollection<Vector2>> LineOrderPreviewChanged; // Make this be invoked when player adjusts the line positions
    
    [Inject] IInputMapper _inputMapper;
    [Inject] IInputManager _inputManager;
    [Inject] ISelectionManager _selectionManager;
    [Inject] IAstarManager _pathfindingAstar;
    [Inject] IDataManager _dataManager;
    [Inject] ICursorManager _cursorManager;

    [SerializeField] private Texture2D interactCursor;

    private void Start()
    {
        _inputMapper.OnWorldPressed2 += OnMoveCommand;
        _inputMapper.OnWorldDragEnd2 += OnLineCommand;
        _inputMapper.OnWorldDrag2 += OnLineCommandPreview;
        _inputManager.Halt += OnHalt;
    }
    
    private void OnDestroy()
    {
        _inputMapper.OnWorldPressed2 -= OnMoveCommand;
        _inputMapper.OnWorldDragEnd2 -= OnLineCommand;
        _inputManager.Halt -= OnHalt;
    }

    private void OnHalt()
    {
        foreach (var creature in GetSelectedCreatures())
        {
            var controller = creature.Controller as UnitController;
            if (controller)
            {
                controller.Halt();
            }
        }
    }

    private void OnMoveCommand(Vector2 position)
    {
        IssueMoveCommand(GetSelectedCreatures().ToList(), position);
    }

    public void IssueMoveCommand(ICollection<Creature> creatures, Vector2 position)
    {
        var entityUnderMouse = _inputMapper.GetEntityUnderMouse();
        if (entityUnderMouse == null)
        {
            MoveCommand(creatures, position);
            return;
        }

        var interactions = entityUnderMouse
            .GetComponents<IInteractable>()
            .OrderByDescending(x => x.Priority)
            .ToArray();

        if (interactions.Any())
        {
            // order selected creatures to first get ones which are not busy,
            // and then the busy ones and then all by distance

            var selectedCreatures = creatures
                .Select(x => x.Controller)
                .OfType<UnitController>()
                .OrderBy(x => x.InteractionTarget != null)
                .ThenBy(x => Vector2.Distance(x.transform.position, entityUnderMouse.transform.position))
                .ToArray();

            foreach (var interaction in interactions)
            {
                foreach (var controller in selectedCreatures)
                {
                    if (interaction.CanInteract(controller.Creature))
                    {
                        controller.SetTarget(interaction);
                        return;
                    }
                }
            }

            selectedCreatures.First().SetTarget(entityUnderMouse);
            return;
        }


        foreach (var creature in GetSelectedCreatures())
        {
            var unitController = creature.Controller as UnitController;
            if (unitController == null)
            {
                GameLogger.LogError("Selected creature is not a unit.");
                continue;
            }

            unitController.SetTarget(entityUnderMouse);
        }
    }
    
    private void OnLineCommand(Vector2 start, Vector2 end)
    {
        var creatures = GetSelectedCreatures().ToList();
        if (creatures.Count == 0)
            return;

        IssueLineMoveCommand(creatures, start, end);
        
        LineOrderPreviewChanged?.Invoke(new List<Vector2>());
    }
    
    private void OnLineCommandPreview(Vector2 start, Vector2 end)
    {
        var creatures = GetSelectedCreatures().ToList();
        if (creatures.Count == 0)
            return;

        // Compute preview
        var preview = GetLinePositions(start, end, creatures.Count)
            .Select(p => _pathfindingAstar.GetClosestValidPosition(p))
            .ToList();

        LineOrderPreviewChanged?.Invoke(preview);
    }

    
    private void IssueLineMoveCommand(ICollection<Creature> creatures, Vector2 start, Vector2 end)
    {
        if (creatures.Count == 1)
        {
            var controller = creatures.First().Controller as UnitController;
            controller?.SetMoveTarget(end);
            LineOrderPreviewChanged?.Invoke(new List<Vector2>());
            return;
        }

        // Evenly spaced positions along line
        var linePositions = GetLinePositions(start, end, creatures.Count);

        // Snap to valid positions (avoid walls/obstacles)
        var snapped = linePositions
            .Select(p => _pathfindingAstar.GetClosestValidPosition(p))
            .ToList();

        // Assign optimally
        var assignments = AssignPositionsToUnits(creatures, snapped);

        foreach (var assignment in assignments)
        {
            var creature = assignment.Key;
            var controller = creature.Controller as UnitController;
            if (controller != null)
            {
                controller.SetMoveTarget(
                    assignment.Value +
                    new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f))
                );
            }
        }

        // Ignore collisions between selected creatures
        foreach (var creature in creatures)
        {
            foreach (var otherCreature in creatures)
            {
                Physics2D.IgnoreCollision(creature.Movement.Collider, otherCreature.Movement.Collider, true);
            }
        }
        
        LineOrderPreviewChanged?.Invoke(new List<Vector2>());
    }



    private void MoveCommand(ICollection<Creature> creatures, Vector2 position)
    {
        if (!creatures.Any())
            return;

        if (creatures.Count == 1)
        {
            var creature = creatures.First();
            var controller = creature.Controller as UnitController;
            if (controller)
            {
                controller.SetMoveTarget(position);
            }

            return;
        }

        var spreadPositions =
            PathfindingUtilities.GetSpreadPosition(position, creatures.Count, LayerMask.GetMask("Walls"), 1f);
        if (!spreadPositions.Any())
        {
            GameLogger.LogWarning("No valid spread positions found. Using rounded position instead.");
            var roundedPosition = (position - Vector2.one).RoundToNearest(0.5f);
            spreadPositions = PathfindingUtilities.GetSpreadPosition(roundedPosition, creatures.Count,
                LayerMask.GetMask("Walls"), 1f);
        }

        if (!spreadPositions.Any())
        {
            GameLogger.LogWarning("No valid spread positions found. Using smaller radius.");
            spreadPositions = PathfindingUtilities.GetSpreadPosition(position, creatures.Count,
                LayerMask.GetMask("Walls"), 0.75f);
        }

        if (!spreadPositions.Any())
        {
            GameLogger.LogWarning("No valid spread positions found.");
            return;
        }


        // Pair each unit with the closest available target position
        var assignments = AssignPositionsToUnits(creatures, spreadPositions);

        foreach (var assignment in assignments)
        {
            var creature = assignment.Key;
            var targetPosition = assignment.Value;

            var controller = creature.Controller as UnitController;
            if (controller)
            {
                controller.SetMoveTarget(targetPosition +
                                         new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)));
            }
        }

        // Disable collision detection for between selected creatures
        foreach (var creature in creatures)
        {
            foreach (var otherCreature in creatures)
            {
                Physics2D.IgnoreCollision(creature.Movement.Collider, otherCreature.Movement.Collider, true);
            }
        }
        
        LineOrderPreviewChanged?.Invoke(new List<Vector2>());
    }

    /// <summary>
    /// Assigns positions to units in a way that minimizes the total distance between units and their assigned positions.
    /// </summary>
    /// <param name="creatures"></param>
    /// <param name="positions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Dictionary<Creature, Vector2> AssignPositionsToUnits(ICollection<Creature> creatures, List<Vector2> positions)
    {
        if (creatures.Count != positions.Count)
            throw new ArgumentException("Number of creatures and positions must be equal.");

        var bestAssignment = new Dictionary<Creature, Vector2>();
        float bestLength = float.MaxValue;

        foreach (var permutation in GetPermutations(positions, positions.Count))
        {
            var assignment = new Dictionary<Creature, Vector2>();
            float totalLength = 0;

            for (int i = 0; i < creatures.Count; i++)
            {
                var creature = creatures.ElementAt(i);
                var position = permutation.ElementAt(i);
                assignment[creature] = position;
                var distance = Vector2.Distance(creature.transform.position, position);
                totalLength += distance * distance;
            }

            if (totalLength < bestLength)
            {
                bestLength = totalLength;
                bestAssignment = new Dictionary<Creature, Vector2>(assignment);
            }
        }

        return bestAssignment;
    }

    private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    {
        if (length == 1) return list.Select(t => new T[] { t });

        return list.SelectMany((t, i) =>
            GetPermutations(list.Where((_, index) => index != i), length - 1)
                .Select(tail => (new T[] { t }).Concat(tail)));
    }

    private IEnumerable<Creature> GetSelectedCreatures()
    {
        return _selectionManager.SelectedCreatures;
    }
    
    private List<Vector2> GetLinePositions(Vector2 start, Vector2 end, int count)
    {
        var positions = new List<Vector2>(count);
        var dir = (end - start).normalized;
        float length = Vector2.Distance(start, end);

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : (float)i / (count - 1);
            positions.Add(start + dir * (length * t));
        }

        return positions;
    }
}