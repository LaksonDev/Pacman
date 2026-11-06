using UnityEngine;

public class Level : MonoBehaviour {

    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform[] ghostSpawnPointArray;

    public Vector2 GetPlayerSpawnPosition() {
        return playerSpawnPoint.position;
    }

    public int GetGhostSpawnPointCount() {
        return ghostSpawnPointArray.Length;
    }

    public Vector2 GetGhostSpawnPosition(int index) {
        return ghostSpawnPointArray[index].position;
    }
}
