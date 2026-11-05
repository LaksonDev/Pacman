using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour {
    
    public static GameManager Instance { get; private set; }

    public event EventHandler OnLevelChanged;

    [SerializeField] private LevelTableSO levelTableSO;
    [SerializeField, Min(1)] private byte startingLevel;

    private byte currentLevel;
    private LevelTier currentTier;

    private void Awake() {
        Instance = this;

        currentLevel = startingLevel;
        currentTier = levelTableSO.GetTierFromLevel(currentLevel);
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void AdvanceLevel() {
        currentLevel++;
        currentTier = levelTableSO.GetTierFromLevel(currentLevel);
        
        OnLevelChanged?.Invoke(this,EventArgs.Empty);
    }

    public int GetCurrentLevel() {
        return currentLevel;
    }

    public LevelTier GetCurrentTier() {
        return currentTier;
    }
}
