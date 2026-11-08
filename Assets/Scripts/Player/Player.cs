using System;
using UnityEngine;

public class Player : MonoBehaviour, ITeleportable {
    
    public static Player Instance { get; private set; }
    
    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);

    private const float PLAYER_DEFAULT_FACING_ANGLE = 0f;

    public event EventHandler OnWalkingChanged;

    [SerializeField] private float movementSpeed = 15f;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask obstaclesLayerMask;

    private Vector2 currentDirection;
    private Vector2 queuedDirection;
    private bool isWalking;
    private bool isMovementLocked;
    private ContactFilter2D obstacleFilter;
    private float speedMultiplier = 1f;

    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];

    private void Awake() {
        Instance = this;

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstaclesLayerMask);
        obstacleFilter.useTriggers = false;
    }

    private void OnEnable() {
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
    }

    private void OnDisable() {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        TeleportTo(e.GetLevel().GetPlayerSpawnPosition());
        ResetState();
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        isMovementLocked = true;
        
        SetWalking(false);
    }

    private void Update() { 
        if (isMovementLocked) return;
        
        HandleMovement();
    }

    private void HandleMovement() {
        Vector2 movementDirection = playerInput.GetMovementVectorNormalized();

        if (movementDirection != Vector2.zero) {
            queuedDirection = movementDirection;
        }

        float movementDistance = movementSpeed * speedMultiplier * Time.deltaTime;

        if (queuedDirection != Vector2.zero && CanMove(queuedDirection, movementDistance)) {
            SetCurrentDirection(queuedDirection);
        }

        bool hasMoved = false;

        if (currentDirection != Vector2.zero && CanMove(currentDirection, movementDistance)) {
            transform.position += (Vector3)(currentDirection * movementDistance);
            hasMoved = true;
        }
        
        SetWalking(hasMoved);
    }

    private void ResetState() {
        currentDirection = Vector2.zero;
        queuedDirection = Vector2.zero;

        transform.rotation = Quaternion.identity;

        isMovementLocked = false;
        SetWalking(false);
    }

    private void SetCurrentDirection(Vector2 direction) {
        if (currentDirection == direction) return;

        currentDirection = direction;
        RotatePlayer(currentDirection);
    }

    private void RotatePlayer(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - PLAYER_DEFAULT_FACING_ANGLE);
    }

    private void SetWalking(bool isWalking) {
        if (this.isWalking == isWalking) return;

        this.isWalking = isWalking;
        OnWalkingChanged?.Invoke(this,EventArgs.Empty);
    }

    private bool CanMove(Vector2 direction, float distance) {
        int hitCount = Physics2D.BoxCast(transform.position, BOX_SIZE, 0f, direction, obstacleFilter, hitBuffer,
            distance);

        return hitCount == 0;
    }

    public void SetSpeedMultiplier(float multiplier) {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public bool GetPlayerIsWalking() {
        return isWalking;
    }

    public void TeleportTo(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}