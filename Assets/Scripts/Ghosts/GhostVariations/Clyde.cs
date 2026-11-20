using UnityEngine;

public class Clyde : BaseGhost {

    private const float SHY_DISTANCE = 8f;

    protected override Vector2 GetChaseTarget() {
        Vector2 playerCell = GetPlayerCell();
        float sqrDistance = (playerCell - GetOwnCell()).sqrMagnitude;

        return sqrDistance > SHY_DISTANCE * SHY_DISTANCE ? playerCell : GetScatterCell();
    }
}