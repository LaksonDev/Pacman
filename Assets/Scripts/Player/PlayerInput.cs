using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {

    public static PlayerInput Instance { get; private set; }

    public event EventHandler OnDirectionPressed;
    public sealed class OnDirectionPressedEventArgs : EventArgs {
        private readonly Vector2 direction;

        public OnDirectionPressedEventArgs(Vector2 direction) {
            this.direction = direction;
        }

        public Vector2 GetDirection() {
            return direction;
        }
    }

    private PlayerInputActions playerInputActions;
    private Vector2 lastDirection;
    
    private int previousRawX;
    private int previousRawY;
    
    private bool isHorizontalAxisPrioritized;
    
    private Vector2 heldDirection;
    
    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        
        playerInputActions.Player.Move.performed += Move_OnPerformed;
        playerInputActions.Player.Move.canceled += Move_OnCanceled;
        
        playerInputActions.Player.Enable();
    }

    private void Move_OnCanceled(InputAction.CallbackContext obj) {
        ProcessRawInput(Vector2.zero);
    }

    private void Move_OnPerformed(InputAction.CallbackContext obj) {
        ProcessRawInput(obj.ReadValue<Vector2>());
    }

    private void OnDestroy() {
        playerInputActions.Player.Move.performed -= Move_OnPerformed;
        playerInputActions.Player.Move.canceled -= Move_OnCanceled;

        playerInputActions.Player.Disable();
        playerInputActions.Dispose();
    }

    private void ProcessRawInput(Vector2 rawInput) {
        int rawX = Math.Sign(rawInput.x);
        int rawY = Math.Sign(rawInput.y);
        
        bool isHorizontalFresh = rawX != 0 && rawX != previousRawX;
        bool isVerticalFresh = rawY != 0 && rawY != previousRawY;
        
        if (isHorizontalFresh && !isVerticalFresh) {
            isHorizontalAxisPrioritized = true;
        } else if (isVerticalFresh && !isHorizontalFresh) {
            isHorizontalAxisPrioritized = false;
        }
        
        if (rawX != 0 && rawY != 0) {
            heldDirection = isHorizontalAxisPrioritized
                ? new Vector2(rawX, 0f)
                : new Vector2(0f, rawY);
        } else {
            heldDirection = new Vector2(rawX, rawY);
        }
        
        if (isHorizontalFresh || isVerticalFresh) {
            OnDirectionPressed?.Invoke(this, new OnDirectionPressedEventArgs(heldDirection));
        }

        previousRawX = rawX;
        previousRawY = rawY;
    }
    
    public Vector2 GetHeldDirection() {
        return heldDirection;
    }
    
    public Vector2 GetMovementVectorNormalized() {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        if (inputVector.x != 0f && inputVector.y != 0f) {
            if (lastDirection.x != 0f) {
                inputVector.y = 0f;
            } else {
                inputVector.x = 0f;
            }
        }

        inputVector = inputVector.normalized;
        lastDirection = inputVector;

        return inputVector;
    }
}