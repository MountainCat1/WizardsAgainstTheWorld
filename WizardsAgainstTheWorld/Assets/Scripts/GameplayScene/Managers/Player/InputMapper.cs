using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Managers;
using Markers;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Zenject;

public interface IInputMapper
{
    event Action<Vector2> OnWorldPressed1;
    event Action<Vector2> OnWorldPressed2;
    event Action<Vector2, Vector2> OnWorldDragEnd2;
    event Action<Vector2, Vector2> OnWorldDrag2;
    event Action<float> Zoom;
    ICollection<Entity> GetEntitiesUnderMouse();
    Entity GetEntityUnderMouse();

    Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    Creature GetCreatureUnderMouse();
    void WaitForFollowUpClick(Action<Vector2> callback, Texture2D cursor = null);
    void CancelFollowUpClick();
}

public class InputMapper : MonoBehaviour, IInputMapper
{
    public event Action<Vector2> OnWorldPressed1;
    public event Action<Vector2> OnWorldPressed2;
    public event Action<Vector2, Vector2> OnWorldDragEnd2;
    public event Action<Vector2, Vector2> OnWorldDrag2;

    public event Action<float> Zoom;

    [Inject] private IInputManager _inputManager;
    [Inject] private ICursorManager _cursorManager;

    private Camera _camera;

    private bool _isRightDragging;
    private Vector2 _rightDragStart;

    private bool _awaitingFollowUpClick;
    private Action<Vector2> _followUpClickCallback;

    [SerializeField] private Texture2D defaultCursor;

    private void OnEnable()
    {
        _camera = Camera.main;

        _inputManager.Pointer2Pressed += OnPointer2Pressed;
        _inputManager.Pointer2Released += OnPointer2Released;
        _inputManager.Pointer1Pressed += OnPointer1Pressed;

        _inputManager.Zoom += OnZoom;
    }


    private void OnDisable()
    {
        _inputManager.Pointer2Pressed -= OnPointer2Pressed;
        _inputManager.Pointer2Released -= OnPointer2Released;
        _inputManager.Pointer1Pressed -= OnPointer1Pressed;

        _inputManager.Zoom -= OnZoom;
    }

    private void OnZoom(float delta)
    {
        if (IsPointerOverUI())
            return;

        Zoom?.Invoke(delta);
    }
    
    private void Update()
    {
        if (_isRightDragging)
        {
            var worldCurrent = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // Fire dragging event
            OnWorldDrag2?.Invoke(_rightDragStart, worldCurrent);
        }
    }


    public void WaitForFollowUpClick(Action<Vector2> callback, Texture2D cursor = null)
    {
        if (_awaitingFollowUpClick)
        {
            GameLogger.LogWarning("Already waiting for a follow-up click!");
            return;
        }

        _cursorManager.SetCursor(this, cursor, CursorPriority.Targeting);
        _awaitingFollowUpClick = true;
        _followUpClickCallback = callback;
    }

    public void CancelFollowUpClick()
    {
        _cursorManager.RemoveCursor(this);
        _awaitingFollowUpClick = false;
        _followUpClickCallback = null;
    }

    private void OnPointer1Pressed(Vector2 position)
    {
        if (IsPointerOverUI())
            return;

        if (_awaitingFollowUpClick)
        {
            _awaitingFollowUpClick = false;
            _cursorManager.RemoveCursor(this);

            // Check for UI click *before* invoking the callback
            if (IsPointerOverUI())
            {
                _followUpClickCallback = null; // Clear the callback
                return; // Don't invoke the callback if it's a UI click
            }

            _followUpClickCallback?.Invoke(_camera.ScreenToWorldPoint(position));
            _followUpClickCallback = null; // Clear the callback
            return;
        }

        var worldPosition = _camera.ScreenToWorldPoint(position);
        OnWorldPressed1?.Invoke(worldPosition);
    }

    private void OnPointer2Pressed(Vector2 position)
    {
        if (IsPointerOverUI())
            return;

        var worldPos = _camera.ScreenToWorldPoint(position);
        _isRightDragging = true;
        _rightDragStart = worldPos;
    }

    private void OnPointer2Released(Vector2 position)
    {
        if (!_isRightDragging) return;
        _isRightDragging = false;

        var worldEnd = _camera.ScreenToWorldPoint(position);

        // Fire drag event if mouse moved enough
        if (Vector2.Distance(_rightDragStart, worldEnd) > 0.15f) // tolerance
        {
            OnWorldDragEnd2?.Invoke(_rightDragStart, worldEnd);
        }
        else
        {
            OnWorldPressed2?.Invoke(worldEnd);
        }
    }

    public ICollection<Entity> GetEntitiesUnderMouse()
    {
        var entities = new List<Entity>();
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            var entity = hit.collider.GetComponent<Entity>();
            if (entity != null)
            {
                entities.Add(entity);
            }
        }

        return entities;
    }

    [CanBeNull]
    public Entity GetEntityUnderMouse()
    {
        // This is not very good, it should use entity collider
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(mousePosition, Vector2.zero); // Raycast in 2D

        if (hit.collider != null)
        {
            var entity = hit.collider.GetComponent<Entity>();
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    [CanBeNull]
    public Creature GetCreatureUnderMouse()
    {
        var mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        var filter = new ContactFilter2D
        {
            useTriggers = true
        };
        filter.SetLayerMask(1 << LayerMask.NameToLayer("CreatureHit")); // Correct way
        filter.useLayerMask = true;

        Collider2D[] results = new Collider2D[1];

        int hitCount = Physics2D.OverlapPoint(mousePosition, filter, results);

        if (hitCount > 0) // Ensure we got a valid hit
        {
            var result = results[0]; // Safe access
            if (result != null)
            {
                var creatureCollider = result.GetComponent<CreatureCollider>();
                if (creatureCollider != null)
                {
                    return creatureCollider.Creature;
                }
            }
        }

        return null;
    }

    private bool IsPointerOverUI()
    {
        return PointerUtilities.IsPointerOverInteractiveUI();
    }
}