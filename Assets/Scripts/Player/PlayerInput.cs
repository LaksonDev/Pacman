using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public static PlayerInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;
    private Vector2 lastDirection;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void OnDestroy() {
        playerInputActions.Player.Disable();
        playerInputActions.Dispose();
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