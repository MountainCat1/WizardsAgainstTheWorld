using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class CursorData
    {
        public Texture2D Cursor;
        public int Priority;
        public object Source;
        public CursorPosition Position;
    }

    public enum CursorPosition
    {
        Center = 0,
        TopLeft = 1,
    }

    public enum CursorPriority
    {
        Default = 0,
        Interactable = 3,
        Targeting = 10,
        Dragging = 5,
    }

    public interface ICursorManager
    {
        void SetCursor(object source, Texture2D cursor, CursorPriority priority,
            CursorPosition position = CursorPosition.Center);

        void RemoveCursor(object source);
    }

    public class CursorManager : MonoBehaviour, ICursorManager
    {
        [Inject] private ISelectionManager _selectionManager;
        
        private readonly List<CursorData> _cursors = new List<CursorData>();

        public void SetCursor(object source, Texture2D cursor, CursorPriority priority,
            CursorPosition position = CursorPosition.Center)
        {
            // TODO: THIS SMELLs WORSE THAN MY ASS
            if (source is InteractionBehavior interactionBehavior)
            {
                if (_selectionManager.SelectedCreatures.All(x => !interactionBehavior.CanInteract(x)))
                {
                    return;
                }
            }
            
            _cursors.Add(new CursorData
            {
                Cursor = cursor,
                Priority = (int)priority,
                Source = source,
                Position = position
            });
            _cursors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            UpdateCursor();
        }

        public void RemoveCursor(object source)
        {
            _cursors.RemoveAll(x => x.Source == source);
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            if (_cursors.Count == 0)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            var cursor = _cursors.OrderByDescending(x => x.Priority).First();
            var hotspot = cursor.Position switch
            {
                CursorPosition.Center => new Vector2(        // ReSharper disable once PossibleLossOfFraction
                    cursor.Cursor.width / 2,                 // ReSharper disable once PossibleLossOfFraction
                    cursor.Cursor.height / 2
                ),
                CursorPosition.TopLeft => Vector2.zero,
                _ => throw new ArgumentOutOfRangeException()
            };

            Cursor.SetCursor(
                cursor.Cursor,
                hotspot,
                CursorMode.Auto
            );
        }

        private void OnDestroy()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _cursors.Clear();
        }

        private void OnDisable()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            _cursors.Clear();
        }
    }
}