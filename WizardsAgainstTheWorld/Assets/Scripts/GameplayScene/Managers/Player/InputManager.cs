using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public interface IInputManager
{
    event Action<Vector2> CameraMovement;
    event Action<Vector2> Pointer1Pressed;
    event Action<Vector2> Pointer1Released;
    event Action<Vector2> Pointer2Pressed;
    event Action<Vector2> Pointer2Released;
    event Action Halt;
    event Action ZoomOnInspectedUnit;
    event Action ChangeInspectedUnit;
    public event Action<Vector2> Pointer1Hold;
    event Action OnConfirm;
    event Action OnSpeedUpDialog;
    event Action OnSkip;
    event Action<float> Zoom;

    event Action SpeedUp;
    event Action SpeedDown;
    event Action TogglePause;
    event Action ToggleJuiceOverdrive;
    event Action GoBackToMenu;
    IUIEvents UI { get; }
    bool IsShiftPressed { get; }
    
    event Action SelectAllUnits;
    event Action SelectUnit1;
    event Action SelectUnit2;
    event Action SelectUnit3;
    event Action SelectUnit4;
    event Action SelectUnit5;
    event Action<InputAction.CallbackContext> Cancel;
}

public interface IUIEvents
{
    public event Action ShowInventory;
    public event Action GoBack;
    public event Action<InputAction.CallbackContext> Cancel;
    public event Action TutorialContinue;
    
}

public class UIEvents : IUIEvents
{
    public event Action ShowInventory;
    public event Action GoBack;
    public event Action<InputAction.CallbackContext> Cancel;
    public event Action TutorialContinue;

    public UIEvents(InputActions inputActions)
    {
        inputActions.UI.ShowInventory.performed += _ => ShowInventory?.SafeInvoke();
        inputActions.UI.GoBack.performed += _ => GoBack?.SafeInvoke();
        inputActions.UI.Cancel.performed += ctx => Cancel?.SafeInvoke(ctx);
        inputActions.UI.TutorialContinue.performed += _ => TutorialContinue?.SafeInvoke();
    }
}

public class InputManager : MonoBehaviour, IInputManager
{
    public event Action<Vector2> CameraMovement;
    public event Action<Vector2> Pointer1Pressed;
    public event Action<Vector2> Pointer1Released;
    public event Action<Vector2> Pointer2Pressed;
    public event Action<Vector2> Pointer2Released;
    public event Action Halt;
    public event Action ZoomOnInspectedUnit;
    public event Action ChangeInspectedUnit;
    public event Action<Vector2> Pointer1Hold;
    public event Action OnConfirm;
    public event Action OnSpeedUpDialog;
    public event Action OnSkip;
    public event Action<float> Zoom;
    public event Action SpeedUp;
    public event Action SpeedDown;
    public event Action TogglePause;
    public event Action ToggleJuiceOverdrive;
    public event Action<InputAction.CallbackContext> Cancel;
    
    public event Action GoBackToMenu;
    public IUIEvents UI { get; private set; }
    public bool IsShiftPressed => Keyboard.current.shiftKey.isPressed;
    
    public event Action SelectAllUnits;
    public event Action SelectUnit1;
    public event Action SelectUnit2;
    public event Action SelectUnit3;
    public event Action SelectUnit4;
    public event Action SelectUnit5;
    

    private InputActions _inputActions;

    private void OnEnable()
    {
        _inputActions = new InputActions();
        _inputActions.Enable();

        _inputActions.CameraControl.Movement.performed +=
            ctx => CameraMovement?.Invoke(ctx.ReadValue<Vector2>());
        _inputActions.CameraControl.Movement.canceled +=
            _ => CameraMovement?.Invoke(Vector2.zero);

        _inputActions.CharacterControl.Pointer1.performed += Pointer1OnPerformed;
        _inputActions.CharacterControl.Pointer1.canceled += Pointer1Canceled;
        _inputActions.CharacterControl.Pointer2.performed += Pointer2OnPerformed;
        _inputActions.CharacterControl.Pointer2.canceled += Pointer2Canceled;
        _inputActions.CharacterControl.Halt.performed += _ => Halt?.SafeInvoke();
        _inputActions.CharacterControl.Cancel.performed += ctx => Cancel?.SafeInvoke(ctx);
        
        _inputActions.CharacterControl.SelectAllUnits.performed += _ => SelectAllUnits?.SafeInvoke();
        _inputActions.CharacterControl.SelectUnit1.performed += _ => SelectUnit1?.SafeInvoke();
        _inputActions.CharacterControl.SelectUnit2.performed += _ => SelectUnit2?.SafeInvoke();
        _inputActions.CharacterControl.SelectUnit3.performed += _ => SelectUnit3?.SafeInvoke();
        _inputActions.CharacterControl.SelectUnit4.performed += _ => SelectUnit4?.SafeInvoke();
        _inputActions.CharacterControl.SelectUnit5.performed += _ => SelectUnit5?.SafeInvoke();

        _inputActions.Misc.TimeSpeedDown.performed += _ => SpeedDown?.SafeInvoke();
        _inputActions.Misc.TimeSpeedUp.performed += _ => SpeedUp?.SafeInvoke();
        _inputActions.Misc.Pause.performed += _ => TogglePause?.SafeInvoke();
        _inputActions.Misc.GoBackToMenu.performed += _ => GoBackToMenu?.SafeInvoke();
        _inputActions.Misc.JuiceOverdriveToggle.performed += _ => ToggleJuiceOverdrive?.SafeInvoke();
        
        _inputActions.UI.Confirm.performed += _ => OnConfirm?.SafeInvoke();
        _inputActions.UI.SkipDialog.performed += _ => OnSkip?.SafeInvoke();
        _inputActions.UI.SpeedUpDialog.performed += _ => OnSpeedUpDialog?.SafeInvoke();
        _inputActions.UI.ChangeInspectedUnit.performed += _ => ChangeInspectedUnit?.SafeInvoke();
        _inputActions.UI.ZoomOnInspectedUnit.performed += _ => ZoomOnInspectedUnit?.SafeInvoke();
        
        _inputActions.UI.ZoomIn.performed += ctx => Zoom?.Invoke(ctx.ReadValue<float>());
        _inputActions.UI.ZoomOut.performed += ctx => Zoom?.Invoke(-ctx.ReadValue<float>());
        
        UI = new UIEvents(_inputActions);
    }

    private void Pointer1Canceled(InputAction.CallbackContext obj)
    {
        Pointer1Released?.Invoke(Mouse.current.position.ReadValue());
    }

    private void Update()
    {
        if(_inputActions.CameraControl.Movement.IsPressed())
        {
            CameraMovement?.Invoke(_inputActions.CameraControl.Movement.ReadValue<Vector2>());
        }
        
        if (_inputActions.CharacterControl.Pointer1.IsPressed())
        {
            Pointer1Hold?.Invoke(Mouse.current.position.ReadValue());
        }
    }

    private void Pointer1OnPerformed(InputAction.CallbackContext callback)
    {
        var pointerPosition = Mouse.current.position;
        Pointer1Pressed?.Invoke(pointerPosition.ReadValue());
    }

    private void Pointer2OnPerformed(InputAction.CallbackContext callback)
    {
        var pointerPosition = Mouse.current.position;
        Pointer2Pressed?.Invoke(pointerPosition.ReadValue());
    }
    
    private void Pointer2Canceled(InputAction.CallbackContext callback)
    {
        var pointerPosition = Mouse.current.position;
        Pointer2Released?.Invoke(pointerPosition.ReadValue());
    }
}