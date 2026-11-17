using UnityEngine;

[CreateAssetMenu(fileName = "GhostModeTimingSO", menuName = "Pacman/GhostModeTimingSO")]
public class GhostModeTimingSO : ScriptableObject {

    [SerializeField, Min(0.1f)] private float[] phaseDurationArray;

    public int GetPhaseCount() {
        return phaseDurationArray.Length;
    }

    public float GetPhaseDuration(int index) {
        return phaseDurationArray[index];
    }
}
