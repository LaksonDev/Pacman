using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour, ITeleportable {

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
    }

    private void OnDisable() {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        currentDirection = Vector2.zero;
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

    private void OnCellCenterReached() {
        bool isNode = Physics2D.OverlapPoint(transform.position, nodeFilter, nodeBuffer) > 0;

        if (!isNode && CanMove(currentDirection)) {
            targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
            return;
        }

        ChooseDirection();
    }

    private void ChooseDirection() {
        int candidateCount = 0;

        foreach (Vector2 direction in DIRECTIONS) {
            if (direction == -currentDirection) continue; 
            if (!CanMove(direction)) continue;

            candidateBuffer[candidateCount++] = direction;
        }

        currentDirection = candidateCount > 0
            ? candidateBuffer[Random.Range(0, candidateCount)]
            : -currentDirection;

        targetPosition = (Vector2)transform.position + currentDirection * CELL_SIZE;
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
}