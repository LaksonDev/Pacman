using UnityEngine;

public class Blinky : BaseGhost {

    protected override Vector2 GetChaseTarget() {
        return GetPlayerCell();
    }
}