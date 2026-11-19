using UnityEngine;

public class Pinky : BaseGhost {

    private const float CELLS_AHEAD = 4f;

    [SerializeField] private bool replicateOverflowBug = true;

    protected override Vector2 GetChaseTarget() {
        return GetCellAheadOfPlayer(CELLS_AHEAD, replicateOverflowBug);
    }
}