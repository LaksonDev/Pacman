using System;
using System.Collections;
using UnityEngine;

public abstract class BaseGhost : MonoBehaviour, ITeleportable, IEdible {

    public static event EventHandler OnAnyGhostCaughtPlayer;
    
    public event EventHandler OnDirectionChanged;
    public event EventHandler OnFrightenedChanged;
    public event EventHandler OnEatenChanged;

    private enum GhostState {
        InHouse,
        Leaving,
        Normal,
        Eaten,
        ReturningToSpawn,
    }

    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);
    private static readonly Vector2[] DIRECTIONS = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

    private const float CELL_SIZE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;

    [SerializeField] private float baseSpeed = 15f;
    [SerializeField] private Vector2 scatterTarget;
    [SerializeField, Min(0)] private int spawnPointIndex;
    [SerializeField] private bool startsInHouse;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask nodesLayerMask;

    private Level currentLevel;
    private GhostState state;
    private Vector2 houseExitPosition;
    private Vector2 spawnPosition;
    private Vector2 currentDirection;
    private Vector2 targetPosition;
    private bool isReverseRequested;
    private bool isFrozen;
    private bool isFrightened;
    private ContactFilter2D obstacleFilter;
    private ContactFilter2D nodeFilter;

    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];
    private readonly Collider2D[] nodeBuffer = new Collider2D[1];
    private readonly Vector2[] candidateBuffer = new Vector2[4];

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
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
        GhostModeScheduler.Instance.OnModeChanged += GhostModeScheduler_OnModeChanged;
        GhostModeScheduler.Instance.OnFrightenedStarted += GhostModeScheduler_OnFrightenedStarted;
        GhostModeScheduler.Instance.OnFrightenedEnded += GhostModeScheduler_OnFrightenedEnded;
        OnAnyGhostCaughtPlayer += BaseGhost_OnAnyGhostCaughtPlayer;
    }

    private void OnDisable() {
        OnAnyGhostCaughtPlayer -= BaseGhost_OnAnyGhostCaughtPlayer;
        
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
            LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        }

        if (GhostModeScheduler.Instance != null) {
            GhostModeScheduler.Instance.OnModeChanged -= GhostModeScheduler_OnModeChanged;
            GhostModeScheduler.Instance.OnFrightenedStarted -= GhostModeScheduler_OnFrightenedStarted;
            GhostModeScheduler.Instance.OnFrightenedEnded -= GhostModeScheduler_OnFrightenedEnded;
        }
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData() {
        OnAnyGhostCaughtPlayer = null;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        bool wasEaten = IsEaten();

        currentLevel = e.GetLevel();
        isFrozen = false;
        isReverseRequested = false;
        SetCurrentDirection(Vector2.zero);
        houseExitPosition = SnapToCellCenter(currentLevel.GetGhostHouseExitPosition());

        spawnPosition = currentLevel.GetGhostSpawnPosition(spawnPointIndex);
        if (!startsInHouse) {
            spawnPosition = SnapToCellCenter(spawnPosition);
        }

        SetPosition(spawnPosition);
        targetPosition = spawnPosition;
        state = startsInHouse ? GhostState.InHouse : GhostState.Normal;

        SetFrightened(false);
        if (wasEaten) OnEatenChanged?.Invoke(this, EventArgs.Empty);

        if (!startsInHouse) {
            StartCoroutine(StartMovingRoutine());
        }
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        isFrozen = true;
    }

    private void BaseGhost_OnAnyGhostCaughtPlayer(object sender, EventArgs e) {
        isFrozen = true;
    }

    private void GhostModeScheduler_OnModeChanged(object sender, GhostModeScheduler.OnModeChangedEventArgs e) {
        if (state != GhostState.Normal) return;

        isReverseRequested = true;
    }

    private void GhostModeScheduler_OnFrightenedStarted(object sender, EventArgs e) {
        if (IsEaten()) return;

        SetFrightened(true);

        if (state == GhostState.Normal) {
            isReverseRequested = true;
        }
    }

    private void GhostModeScheduler_OnFrightenedEnded(object sender, EventArgs e) {
        SetFrightened(false);
    }

    private IEnumerator StartMovingRoutine() {
        yield return new WaitForFixedUpdate();

        ChooseDirection();
    }

    private void Update() {
        if (isFrozen) return;

        switch (state) {
            case GhostState.Leaving:
                UpdateLeaving();
                break;
            case GhostState.Normal:
            case GhostState.Eaten:
                UpdateGridMovement();
                break;
            case GhostState.ReturningToSpawn:
                UpdateReturningToSpawn();
                break;
        }
    }

    private void UpdateGridMovement() {
        if (currentDirection == Vector2.zero) return;

        float distance = GetCurrentSpeed() * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);

        while (distance > 0f) {
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            if (distanceToTarget > distance) {
                transform.position += (Vector3)(currentDirection * distance);
                return;
            }

            SetPosition(targetPosition);
            distance -= distanceToTarget;
            OnCellCenterReached();

            if (state == GhostState.ReturningToSpawn) return;
        }
    }

    private void UpdateLeaving() {
        float distance = GetCurrentSpeed() * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);
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

    private void UpdateReturningToSpawn() {
        float distance = GetCurrentSpeed() * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);
        Vector2 position = transform.position;

        bool isOnSpawnRow = Mathf.Approximately(position.y, spawnPosition.y);
        Vector2 waypoint = isOnSpawnRow ? spawnPosition : new Vector2(position.x, spawnPosition.y);

        if (isOnSpawnRow) {
            SetCurrentDirection(spawnPosition.x > position.x ? Vector2.right : Vector2.left);
        } else {
            SetCurrentDirection(spawnPosition.y > position.y ? Vector2.up : Vector2.down);
        }

        Vector2 next = Vector2.MoveTowards(position, waypoint, distance);
        SetPosition(next);

        if (next != spawnPosition) return;

        state = GhostState.Leaving;
        OnEatenChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FinishLeaving() {
        state = GhostState.Normal;

        ChooseDirection();
    }

    public void ReleaseFromHouse() {
        if (state != GhostState.InHouse) return;

        state = GhostState.Leaving;
    }

    private float GetCurrentSpeed() {
        LevelTier tier = GameManager.Instance.GetCurrentTier();

        float multiplier;
        if (IsEaten()) {
            multiplier = tier.GetGhostEatenSpeedMultiplier();
        } else if (currentLevel.IsInTunnel(transform.position)) {
            multiplier = tier.GetGhostTunnelSpeedMultiplier();
        } else if (isFrightened) {
            multiplier = tier.GetGhostFrightenedSpeedMultiplier();
        } else {
            multiplier = tier.GetGhostNormalSpeedMultiplier();
        }

        return baseSpeed * multiplier;
    }

    private void SetCurrentDirection(Vector2 direction) {
        if (currentDirection == direction) return;

        currentDirection = direction;
        OnDirectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetFrightened(bool value) {
        if (isFrightened == value) return;

        isFrightened = value;
        OnFrightenedChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsFrightened() {
        return isFrightened;
    }

    public bool IsEaten() {
        return state == GhostState.Eaten || state == GhostState.ReturningToSpawn;
    }

    public void Eat() {
        if (isFrozen || IsEaten()) return;

        if (!isFrightened) {
            OnAnyGhostCaughtPlayer?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (state != GhostState.Normal) return;

        state = GhostState.Eaten;
        isReverseRequested = false;

        SetFrightened(false);
        OnEatenChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCellCenterReached() {
        if (state == GhostState.Eaten && (Vector2)transform.position == houseExitPosition) {
            state = GhostState.ReturningToSpawn;
            return;
        }

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
        Vector2 chosenDirection = state == GhostState.Normal && isFrightened
            ? PickRandomDirection()
            : PickDirectionTowardsTarget();

        SetCurrentDirection(chosenDirection != Vector2.zero ? chosenDirection : -currentDirection);
        targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
    }

    private Vector2 PickDirectionTowardsTarget() {
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

        return bestDirection;
    }

    private Vector2 PickRandomDirection() {
        int candidateCount = 0;

        foreach (Vector2 direction in DIRECTIONS) {
            if (direction == -currentDirection) continue;
            if (!CanMove(direction)) continue;

            candidateBuffer[candidateCount++] = direction;
        }

        return candidateCount > 0 ? candidateBuffer[UnityEngine.Random.Range(0, candidateCount)] : Vector2.zero;
    }

    private Vector2 GetTargetPosition() {
        if (state == GhostState.Eaten) return houseExitPosition;

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