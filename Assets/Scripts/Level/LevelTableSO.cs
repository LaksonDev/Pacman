using UnityEngine;

[CreateAssetMenu(fileName = "LevelTableSO", menuName = "Pacman/LevelTableSO")]
public class LevelTableSO : ScriptableObject {

    [SerializeField] private LevelTier[] levelTiers;

    public LevelTier GetTierFromLevel(int level) {
        bool found = false;
        LevelTier best = default;

        foreach (LevelTier levelTier in levelTiers) {
            if (levelTier.GetFromLevel() > level) continue;
            if (found && levelTier.GetFromLevel() <= best.GetFromLevel()) continue;

            best = levelTier;
            found = true;
        }
        
        return best;
    }
}
