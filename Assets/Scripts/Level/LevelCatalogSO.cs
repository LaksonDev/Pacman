using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalogSO", menuName = "Pacman/LevelCatalogSO")]
public class LevelCatalogSO : ScriptableObject {

    [SerializeField] private Level[] levelPrefabArray;

    public int GetLevelCount() {
        return levelPrefabArray.Length;
    }

    public Level GetLevelPrefab(int levelNumber) {
        int count = levelPrefabArray.Length;
        
        int mapIndex = ((levelNumber % count) + count) % count;

        return levelPrefabArray[mapIndex];
    }
}
