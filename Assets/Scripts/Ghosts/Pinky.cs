using UnityEngine;

public class Pinky : BaseGhost {
    
    private const float CELLS_AHEAD = 4f;
    
    [SerializeField] private bool replicateOverflowBug = true;
    
    protected override Vector2 GetChaseTarget() {
        Vector2 playerDirection = GetPlayerDirection();
        Vector2 target = GetPlayerCell() + playerDirection * CELLS_AHEAD;

        if (replicateOverflowBug && playerDirection == Vector2.up) {
            target += Vector2.left * CELLS_AHEAD;
        }

        return target;
    }
}