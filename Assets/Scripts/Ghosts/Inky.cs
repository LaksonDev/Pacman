using UnityEngine;

public class Inky : BaseGhost {
    
    private const float CELLS_AHEAD = 2f;

    [SerializeField] private Blinky blinky;
    [SerializeField] private bool replicateOverflowBug = true;


    protected override Vector2 GetChaseTarget() {
        Vector2 pivot = GetCellAheadOfPlayer(CELLS_AHEAD, replicateOverflowBug);

        if (blinky == null) return pivot;
        
        Vector2 blinkyCell = SnapToCellCenter(blinky.transform.position);
        return pivot + (pivot - blinkyCell);
    }
}
