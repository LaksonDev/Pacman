using System;
using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour, ITeleportable {

    public event EventHandler OnDirectionChanged;

    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);
    private static readonly Vector2[] DIRECTIONS = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

    private const float CELL_SIZE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;

    [SerializeField] private float movementSpeed = 11f;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask nodesLayerMask;

    private Vector2 currentDirection;
    private Vector2 targetPosition;
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
    }

    private void OnDisable() {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        SetCurrentDirection(Vector2.zero);
        TeleportTo(e.GetLevel().GetGhostSpawnPosition(0));
        StartCoroutine(StartMovingRoutine());
    }

    private IEnumerator StartMovingRoutine() {
        yield return new WaitForFixedUpdate();

        ChooseDirection();
    }

    private void Update() {
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
    
    private void SetCurrentDirection(Vector2 direction) {
        if (currentDirection == direction) return;

        currentDirection = direction;
        OnDirectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCellCenterReached() {
        bool isNode = Physics2D.OverlapPoint(transform.position, nodeFilter, nodeBuffer) > 0;

        if (!isNode && CanMove(currentDirection)) {
            targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
            return;
        }

        ChooseDirection();
    }

    private void ChooseDirection() {
        Vector2 chaseTarget = GetChaseTarget();

        Vector2 bestDirection = Vector2.zero;
        float bestDistance = float.MaxValue;

        foreach (Vector2 direction in DIRECTIONS) {
            if (direction == -currentDirection) continue;
            if (!CanMove(direction)) continue;

            Vector2 nextCell = (Vector2)transform.position + direction * CELL_SIZE;
            float distance = (chaseTarget - nextCell).sqrMagnitude;

            if (distance >= bestDistance) continue;

            bestDistance = distance;
            bestDirection = direction;
        }

        SetCurrentDirection(bestDirection != Vector2.zero ? bestDirection : -currentDirection);
        targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
    }

    private Vector2 GetChaseTarget() {
        if (Player.Instance == null) return transform.position;

        return SnapToCellCenter(Player.Instance.transform.position);
    }

    private bool CanMove(Vector2 direction) {
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

    public void TeleportTo(Vector2 position) {

        Vector2 center = SnapToCellCenter(position);

        SetPosition(center);
        targetPosition = center;
    }
    
    public Vector2 GetCurrentDirection() {
        return currentDirection;
    }
}