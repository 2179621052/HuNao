using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;

    [SerializeField] private bool player2Controls;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();

        if (player2Controls)
        {
            ApplyPlayer2BindingOverrides();
        }

        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternative.performed += InteractAlternate_performed;
    }

    private void ApplyPlayer2BindingOverrides()
    {
        OverrideBinding(playerInputActions.Player.Move, "up", "<Keyboard>/upArrow");
        OverrideBinding(playerInputActions.Player.Move, "down", "<Keyboard>/downArrow");
        OverrideBinding(playerInputActions.Player.Move, "left", "<Keyboard>/leftArrow");
        OverrideBinding(playerInputActions.Player.Move, "right", "<Keyboard>/rightArrow");

        playerInputActions.Player.Interact.ApplyBindingOverride(0, "<Keyboard>/numpad1");
        playerInputActions.Player.InteractAlternative.ApplyBindingOverride(0, "<Keyboard>/numpad2");
    }

    private static void OverrideBinding(InputAction action, string bindingName, string bindingPath)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].name == bindingName)
            {
                action.ApplyBindingOverride(i, bindingPath);
                return;
            }
        }
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }

    private void OnDestroy()
    {
        if (playerInputActions == null)
        {
            return;
        }

        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternative.performed -= InteractAlternate_performed;
        if (Application.isPlaying)
        {
            playerInputActions.Player.Disable();
            playerInputActions.Dispose();
        }
    }
}
