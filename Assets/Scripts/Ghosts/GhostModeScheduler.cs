using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class GhostModeScheduler : MonoBehaviour {
    
    public static GhostModeScheduler Instance { get; private set; }

    public enum GhostMode {
        Scatter,
        Chase
    }

    public event EventHandler<OnModeChangedEventArgs> OnModeChanged;
    public sealed class OnModeChangedEventArgs : EventArgs {
        private readonly GhostMode mode;

        public OnModeChangedEventArgs(GhostMode mode) {
            this.mode = mode;
        }

        public GhostMode GetMode() {
            return mode;
        }
    }

    [SerializeField] private GhostModeTimingSO ghostModeTimingSO;
    
    private GhostMode currentGhostMode = GhostMode.Scatter;
    private Coroutine scheduleRoutine;

    private void Awake() {
        Instance = this;
    }

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
    }
    
    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        if (scheduleRoutine != null) {
            StopCoroutine(scheduleRoutine);
        }
        
        currentGhostMode = GhostMode.Scatter;
        scheduleRoutine = StartCoroutine(ScheduleRoutine());
    }

    private IEnumerator ScheduleRoutine() {
        for (int i = 0; i < ghostModeTimingSO.GetPhaseCount(); i++) {
            SetMode(i % 2 == 0 ? GhostMode.Scatter : GhostMode.Chase);

            yield return new WaitForSeconds(ghostModeTimingSO.GetPhaseDuration(i));
        }

        SetMode(GhostMode.Chase);
    }
    
    private void SetMode(GhostMode mode) {
        if (currentGhostMode == mode) return;

        currentGhostMode = mode;
        OnModeChanged?.Invoke(this, new OnModeChangedEventArgs(mode));
    }

    public GhostMode GetCurrentMode() {
        return currentGhostMode;
    }
}
