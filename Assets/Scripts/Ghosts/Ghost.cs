using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour, ITeleportable {
    
    private enum Direction {
        None,
        Up,
        Left,
        Down,
        Right
    }

    private static readonly Vector2 BOX_SIZE = new Vector2(.8f, .8f);
    private static readonly Direction[] DIRECTIONS = { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
    
    private const float CELL_SIZE = 1f;
    private const float MAX_FRAME_TIME = 0.05f;

    [SerializeField] private float movementSpeed = 11f;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask nodesLayerMask;

    private Direction currentDirection;
    private Vector2 targetNodePosition;
    private bool hasTargetNode;
    private ContactFilter2D obstacleFilter;
    
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[1];
    private readonly Direction[] candidateBuffer = new Direction[4];

    private void Awake() {
        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstaclesLayerMask);
        obstacleFilter.useTriggers = false;
    }

    private void Update() {
        if (currentDirection == Direction.None) return;
        
        float distance = movementSpeed * Mathf.Min(Time.deltaTime, MAX_FRAME_TIME);

        if (hasTargetNode) {
            float distanceToNode = Vector2.Dot(targetNodePosition - (Vector2)transform.position,
                GetVector(currentDirection));

            if (distanceToNode <= distance) {
                TeleportTo(targetNodePosition);
                ChooseDirection();
                distance -= Mathf.Max(0f, distanceToNode);
            }
        }
        
        transform.position += (Vector3)(GetVector(currentDirection) * distance);
    }

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
    }
    
    private void OnDisable() {
        if (LevelManager.Instance == null) return;

        LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        currentDirection = Direction.None;
        TeleportTo(e.GetLevel().GetGhostSpawnPosition(0));
        StartCoroutine(StartMovingRoutine());
    }

    private IEnumerator StartMovingRoutine() {
        yield return new WaitForFixedUpdate();

        ChooseDirection();
    }

    private void OnTriggerEnter2D(Collider2D collider2D) {
        if ((nodesLayerMask.value & (1 << collider2D.gameObject.layer)) == 0) return;

        Vector2 nodePosition = collider2D.transform.position;
        
        if (Vector2.Dot(nodePosition - (Vector2)transform.position, GetVector(currentDirection)) <= 0f) return;

        targetNodePosition = nodePosition;
        hasTargetNode = true;
    }

    private void ChooseDirection() {
        int candidateCount = 0;
        Direction backwards = GetOppositeDirection(currentDirection);

        foreach (Direction direction in DIRECTIONS) {
            if (direction==backwards) continue;
            if (!CanMove(GetVector(direction), CELL_SIZE)) continue;

            candidateBuffer[candidateCount++] = direction;
        }
        
        currentDirection = candidateCount > 0
            ? candidateBuffer[Random.Range(0, candidateCount)]
            : backwards;
    }
    
    private bool CanMove(Vector2 direction, float distance) {
        int hitCount = Physics2D.BoxCast(transform.position, BOX_SIZE, 0f, direction, obstacleFilter, hitBuffer,
            distance);

        return hitCount == 0;
    }

    private static Vector2 GetVector(Direction direction) {
        switch (direction) {
            case Direction.Up:
                return Vector2.up;
                break;
            case Direction.Down:
                return Vector2.down;
                break;
            case Direction.Left:
                return Vector2.left;
                break;
            case Direction.Right:
                return Vector2.right;
                break;
            default:
                return Vector2.zero;
        }
    }
    
    private static Direction GetOppositeDirection(Direction direction) {
        switch (direction) {
            case Direction.Up:
                return Direction.Down;
                break;
            case Direction.Down:
                return Direction.Up;
                break;
            case Direction.Left:
                return Direction.Right;
                break;
            case Direction.Right:
                return Direction.Left;
                break;
            default:
                return Direction.None;
        }
    }
    
    public void TeleportTo(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        hasTargetNode = false;
    }
}
