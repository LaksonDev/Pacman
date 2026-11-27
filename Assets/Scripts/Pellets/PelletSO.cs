using UnityEngine;

[CreateAssetMenu(fileName = "PelletSO", menuName = "Pacman/PelletSO")]
public class PelletSO : ScriptableObject {

    [SerializeField, Min(0)] private int points;
    [SerializeField] private bool isPowerPellet;

    public int GetPoints() {
        return points;
    }

    public bool IsPowerPellet() {
        return isPowerPellet;
    }
}