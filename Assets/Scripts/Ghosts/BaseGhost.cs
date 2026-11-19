using System;
using System.Collections;
using UnityEngine;

public abstract class BaseGhost : MonoBehaviour, ITeleportable {

    public event EventHandler OnDirectionChanged;

    private enum GhostState {
        InHouse,
        Leaving,
        Normal,
    }

    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);
    private static readonly Vector2[] DIRECTIONS = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

    private const float CELL_SIZE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;

    [SerializeField] private float movementSpeed = 11f;
    [SerializeField] private Vector2 scatterTarget;
    [SerializeField, Min(0)] private int spawnPointIndex;
    [SerializeField] private bool startsInHouse;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask nodesLayerMask;

    private GhostState state;
    private Vector2 houseExitPosition;
    private Vector2 currentDirection;
    private Vector2 targetPosition;
    private bool isReverseRequested;
    private ContactFilter2D obstacleFilter;
    private ContactFilter2D nodeFilter;

    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];
    private readonly Collider2D[] nodeBuffer = new Collider2D[1];

    private void Awake() {
        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstaclesLayerMask);
        obstacleFilter.useTriggers = false;

        nodeFilter = new ContactFilter2D();
        nodeFilter.SetLayerMask(nodesLayerMask);
        nodeFilter.useTriggers = true;
    }

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
        GhostModeScheduler.Instance.OnModeChanged += GhostModeScheduler_OnModeChanged;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }

        if (GhostModeScheduler.Instance != null) {
            GhostModeScheduler.Instance.OnModeChanged -= GhostModeScheduler_OnModeChanged;
        }
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        isReverseRequested = false;
        SetCurrentDirection(Vector2.zero);
        TeleportTo(e.GetLevel().GetGhostSpawnPosition(spawnPointIndex));
        houseExitPosition = SnapToCellCenter(e.GetLevel().GetGhostHouseExitPosition());

        if (startsInHouse) {
            state = GhostState.InHouse;
            return;
        }

        state = GhostState.Normal;
        StartCoroutine(StartMovingRoutine());
    }

    private void GhostModeScheduler_OnModeChanged(object sender, GhostModeScheduler.OnModeChangedEventArgs e) {
        if (state != GhostState.Normal) return;

        isReverseRequested = true;
    }

    private IEnumerator StartMovingRoutine() {
        yield return new WaitForFixedUpdate();

        ChooseDirection();
    }

    private void Update() {
        switch (state) {
            case GhostState.Leaving:
                UpdateLeaving();
                break;
            case GhostState.Normal:
                UpdateNormalMovement();
                break;
        }
    }

    private void UpdateNormalMovement() {
        if (currentDirection == Vector2.zero) return;

        float distance = movementSpeed * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);

        while (distance > 0f) {
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distanceToTarget > distance) {
                transform.position += (Vector3)(currentDirection * distance);
                return;
            }

            SetPosition(targetPosition);
            distance -= distanceToTarget;
            OnCellCenterReached();
        }
    }

    private void UpdateLeaving() {
        float distance = movementSpeed * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);
        Vector2 position = transform.position;

        bool isInExitColumn = Mathf.Approximately(position.x, houseExitPosition.x);
        Vector2 waypoint = isInExitColumn ? houseExitPosition : new Vector2(houseExitPosition.x, position.y);

        if (isInExitColumn) {
            SetCurrentDirection(Vector2.up);
        } else {
            SetCurrentDirection(houseExitPosition.x > position.x ? Vector2.right : Vector2.left);
        }

        Vector2 next = Vector2.MoveTowards(position, waypoint, distance);
        SetPosition(next);

        if (next == houseExitPosition) {
            FinishLeaving();
        }
    }

    private void FinishLeaving() {
        state = GhostState.Normal;

        ChooseDirection();
    }

    public void ReleaseFromHouse() {
        if (state != GhostState.InHouse) return;

        state = GhostState.Leaving;
    }

    private void SetCurrentDirection(Vector2 direction) {
        if (currentDirection == direction) return;

        currentDirection = direction;
        OnDirectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCellCenterReached() {
        if (isReverseRequested) {
            isReverseRequested = false;
            SetCurrentDirection(-currentDirection);
            targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
            return;
        }

        bool isNode = Physics2D.OverlapPoint(transform.position, nodeFilter, nodeBuffer) > 0;

        if (!isNode && CanMove(currentDirection)) {
            targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
            return;
        }

        ChooseDirection();
    }

    private void ChooseDirection() {
        Vector2 target = GetTargetPosition();

        Vector2 bestDirection = Vector2.zero;
        float bestDistance = float.MaxValue;

        foreach (Vector2 direction in DIRECTIONS) {
            if (direction == -currentDirection) continue;
            if (!CanMove(direction)) continue;

            Vector2 nextCell = (Vector2)transform.position + direction * CELL_SIZE;
            float distance = (target - nextCell).sqrMagnitude;

            if (distance >= bestDistance) continue;

            bestDistance = distance;
            bestDirection = direction;
        }

        SetCurrentDirection(bestDirection != Vector2.zero ? bestDirection : -currentDirection);
        targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
    }

    private Vector2 GetTargetPosition() {
        bool isScatter = GhostModeScheduler.Instance.GetCurrentMode() == GhostModeScheduler.GhostMode.Scatter;

        return isScatter ? GetScatterCell() : GetChaseTarget();
    }

    protected abstract Vector2 GetChaseTarget();

    protected Vector2 GetPlayerCell() {
        if (Player.Instance == null) return transform.position;

        return SnapToCellCenter(Player.Instance.transform.position);
    }

    protected Vector2 GetPlayerDirection() {
        return Player.Instance == null ? Vector2.zero : Player.Instance.GetCurrentDirection();
    }
    
    protected Vector2 GetScatterCell() {
        return SnapToCellCenter(scatterTarget);
    }

    protected Vector2 GetOwnCell() {
        return SnapToCellCenter(transform.position);
    }

    protected Vector2 GetCellAheadOfPlayer(float cells, bool replicateOverflowBug) {
        Vector2 direction = GetPlayerDirection();
        Vector2 target = GetPlayerCell() + direction * cells;

        if (replicateOverflowBug && direction == Vector2.up) {
            target += Vector2.left * cells;
        }

        return target;
    }

    private bool CanMove(Vector2 direction) {
        int hitCount = Physics2D.BoxCast(transform.position, BOX_SIZE, 0f, direction, obstacleFilter, hitBuffer,
            CELL_SIZE);

        return hitCount == 0;
    }

    private void SetPosition(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    protected static Vector2 SnapToCellCenter(Vector2 position) {
        return new Vector2(Mathf.Floor(position.x) + 0.5f, Mathf.Floor(position.y) + 0.5f);
    }

    public void TeleportTo(Vector2 position) {
        Vector2 center = SnapToCellCenter(position);

        SetPosition(center);
        targetPosition = center;
    }

    public Vector2 GetCurrentDirection() {
        return currentDirection;
    }
}