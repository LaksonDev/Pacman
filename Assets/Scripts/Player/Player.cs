using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour, ITeleportable {

    public static Player Instance { get; private set; }
    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);

    public event EventHandler OnLifeLost;
    public event EventHandler OnGameOver;

    private const float PLAYER_DEFAULT_FACING_ANGLE = 0f;
    private const float CELL_SIZE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;
    private const byte STARTING_LIVES = 3;

    public event EventHandler OnWalkingChanged;

    [SerializeField] private float movementSpeed = 15f;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask obstaclesLayerMask;

    private Vector2 currentDirection;
    private Vector2 queuedDirection;
    private Vector2 targetPosition;
    private bool isStopped = true;
    private bool isWalking;
    private bool isMovementLocked;
    private ContactFilter2D obstacleFilter;
    private float speedMultiplier = 1f;
    private byte playerLives = STARTING_LIVES;

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
        BaseGhost.OnAnyGhostCaughtPlayer += BaseGhost_OnAnyGhostCaughtPlayer;
    }

    private void OnDisable() {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        BaseGhost.OnAnyGhostCaughtPlayer -= BaseGhost_OnAnyGhostCaughtPlayer;
    }
    
    private void BaseGhost_OnAnyGhostCaughtPlayer(object sender, EventArgs e) {
        if (isMovementLocked) return;

        isMovementLocked = true;
        SetWalking(false);

        LoseLife();
    }

    private void LoseLife() {
        if (playerLives == 0) return;

        playerLives--;

        if (playerLives == 0) {
            OnGameOver?.Invoke(this, EventArgs.Empty);
            return;
        }

        OnLifeLost?.Invoke(this, EventArgs.Empty);
    }

    public void ResetLives() {
        playerLives = STARTING_LIVES;
    }

    public byte GetLives() {
        return playerLives;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        SetPosition(e.GetLevel().GetPlayerSpawnPosition());
        targetPosition = transform.position;
        ResetState();
        StartCoroutine(UnlockMovementRoutine());
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        isMovementLocked = true;

        SetWalking(false);
    }

    private IEnumerator UnlockMovementRoutine() {
        yield return new WaitForFixedUpdate();

        isMovementLocked = false;
    }

    private void Update() {
        if (isMovementLocked) return;

        HandleMovement();
    }

    private void HandleMovement() {
        ReadInput();
        TryReverse();

        if (isStopped) {
            DecideAtCellCenter();
        }

        float distance = movementSpeed * speedMultiplier * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);
        bool hasMoved = false;

        while (!isStopped && distance > 0f) {
            hasMoved = true;

            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distanceToTarget > distance) {
                transform.position += (Vector3)(currentDirection * distance);
                break;
            }

            SetPosition(targetPosition);
            distance -= distanceToTarget;
            DecideAtCellCenter();
        }

        SetWalking(hasMoved);
    }

    private void ReadInput() {
        Vector2 heldDirection = playerInput.GetHeldDirection();

        if (heldDirection != Vector2.zero) {
            queuedDirection = heldDirection;
        }
    }

    private void TryReverse() {
        if (isStopped || queuedDirection == Vector2.zero) return;
        if (queuedDirection != -currentDirection) return;

        SetCurrentDirection(queuedDirection);
        targetPosition += currentDirection * CELL_SIZE;
    }

    private void DecideAtCellCenter() {
        if (queuedDirection != Vector2.zero && CanMove(queuedDirection)) {
            SetCurrentDirection(queuedDirection);
        } else if (!CanMove(currentDirection)) {
            isStopped = true;
            return;
        }

        isStopped = false;
        targetPosition = GetNextCellCenter(transform.position, currentDirection);
    }

    private void ResetState() {
        currentDirection = Vector2.zero;
        queuedDirection = Vector2.zero;
        isStopped = true;

        transform.rotation = Quaternion.identity;

        isMovementLocked = true;
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
        OnWalkingChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool CanMove(Vector2 direction) {
        if (direction == Vector2.zero) return false;

        int hitCount = Physics2D.BoxCast(transform.position, BOX_SIZE, 0f, direction, obstacleFilter, hitBuffer,
            CELL_SIZE);

        return hitCount == 0;
    }

    private void SetPosition(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private static Vector2 SnapToCellCenter(Vector2 position) {
        return new Vector2(Mathf.Floor(position.x) + 0.5f, Mathf.Floor(position.y) + 0.5f);
    }

    private static Vector2 GetNextCellCenter(Vector2 position, Vector2 direction) {
        if (direction.x > 0f) return new Vector2(Mathf.Floor(position.x - 0.5f) + 1.5f, position.y);
        if (direction.x < 0f) return new Vector2(Mathf.Ceil(position.x - 0.5f) - 0.5f, position.y);
        if (direction.y > 0f) return new Vector2(position.x, Mathf.Floor(position.y - 0.5f) + 1.5f);

        return new Vector2(position.x, Mathf.Ceil(position.y - 0.5f) - 0.5f);
    }

    public void SetSpeedMultiplier(float multiplier) {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public bool GetPlayerIsWalking() {
        return isWalking;
    }

    public Vector2 GetCurrentDirection() {
        return currentDirection;
    }

    public void TeleportTo(Vector2 position) {
        Vector2 center = SnapToCellCenter(position);

        SetPosition(center);
        targetPosition = center;
        isStopped = true;
    }
}