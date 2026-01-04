using UnityEngine;
using Zenject;

namespace CreatureControllers
{
    public class PlayerController : CreatureController
    {
        [Inject] private IInputManager _inputManager;

        private void Start()
        {
            _inputManager.CameraMovement += OnCameraMovement;
        }

        private void OnCameraMovement(Vector2 direction)
        {
            Creature.Movement.SetMovement(direction);
        }
    }
}