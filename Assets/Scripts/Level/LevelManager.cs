using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LevelManager : MonoBehaviour {
    
    public static LevelManager Instance { get; private set; }

    public event EventHandler<OnLevelLoadedEventArgs> OnLevelLoaded;
    public sealed class OnLevelLoadedEventArgs : EventArgs {
        private readonly Level level;
        private readonly int levelNumber;

        public OnLevelLoadedEventArgs(Level level, int levelNumber) {
            this.level = level;
            this.levelNumber = levelNumber;
        }

        public Level GetLevel() {
            return level;
        }

        public int GetLevelNumber() {
            return levelNumber;
        }
    }

    public event EventHandler<OnLevelCompletedEventArgs> OnLevelCompleted;
    public sealed class OnLevelCompletedEventArgs : EventArgs {
        private readonly int levelNumber;

        public OnLevelCompletedEventArgs(int levelNumber) {
            this.levelNumber = levelNumber;
        }

        public int GetLevelNumber() {
            return levelNumber;
        }
    }

    [SerializeField] private LevelCatalogSO levelCatalogSO;
    [SerializeField] private PelletCounter pelletCounter;
    [SerializeField, Min(0)] private int startingLevelNumber;

    private Level currentLevel;
    private int currentLevelNumber;
    private bool isTransitioning;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (isTransitioning) return;
        isTransitioning = true;
        
        OnLevelCompleted?.Invoke(this, new OnLevelCompletedEventArgs(currentLevelNumber));
        StartCoroutine(LoadNextLevelRoutine());
    }

    private IEnumerator LoadNextLevelRoutine() {
        yield return null;
        
        LoadLevel(currentLevelNumber + 1);
        isTransitioning = false;
    }

    private void LoadLevel(int levelNumber) {
        Level levelPrefab = levelCatalogSO.GetLevelPrefab(levelNumber);
        if (levelPrefab == null) return;
        
        UnloadCurrentLevel();
        
        pelletCounter.ResetCount();

        currentLevel = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
        currentLevel.name = levelPrefab.name;
        currentLevelNumber = levelNumber;
        
        OnLevelLoaded?.Invoke(this, new OnLevelLoadedEventArgs(currentLevel, currentLevelNumber));
    }

    private void UnloadCurrentLevel() {
        if (currentLevel == null) return;
        
        currentLevel.gameObject.SetActive(false);
        Destroy(currentLevel.gameObject);
        currentLevel = null;
    }

    private void OnEnable() {
        pelletCounter.OnAllPelletsEaten += PelletCounter_OnAllPelletsEaten;
    }

    private void OnDisable() {
        pelletCounter.OnAllPelletsEaten -= PelletCounter_OnAllPelletsEaten;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void PelletCounter_OnAllPelletsEaten(object sender, EventArgs e) {
        if (isTransitioning) return;
        isTransitioning = true;
        
        OnLevelCompleted?.Invoke(this, new OnLevelCompletedEventArgs(currentLevelNumber));
        StartCoroutine(LoadNextLevelRoutine());
    }

    public Level GetCurentLevel() {
        return currentLevel;
    }

    public int GetCurrentLevelNumber() {
        return currentLevelNumber;
    }
}
