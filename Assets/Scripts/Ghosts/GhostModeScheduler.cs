using System;
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

    public event EventHandler OnFrightenedStarted;
    public event EventHandler OnFrightenedEnded;

    [SerializeField] private GhostModeTimingSO ghostModeTimingSO;

    private GhostMode currentGhostMode = GhostMode.Scatter;
    private int phaseIndex;
    private float phaseTimeLeft;
    private bool isRunning;
    private bool isFrightened;
    private float frightenedTimeLeft;

    private void Awake() {
        Instance = this;
    }

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
        Pellet.OnAnyPowerPelletEaten += Pellet_OnAnyPowerPelletEaten;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
            LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        }

        Pellet.OnAnyPowerPelletEaten -= Pellet_OnAnyPowerPelletEaten;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        currentGhostMode = GhostMode.Scatter;
        isFrightened = false;
        phaseIndex = 0;
        phaseTimeLeft = ghostModeTimingSO.GetPhaseCount() > 0 ? ghostModeTimingSO.GetPhaseDuration(0) : 0f;
        isRunning = true;
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        isRunning = false;
    }

    private void Pellet_OnAnyPowerPelletEaten(object sender, EventArgs e) {
        if (!isRunning) return;

        float duration = GameManager.Instance.GetCurrentTier().GetFrightenedDuration();
        if (duration <= 0f) return;

        isFrightened = true;
        frightenedTimeLeft = duration;
        Debug.Log($"Frightened started for {duration}s");
        OnFrightenedStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        if (!isRunning) return;

        if (isFrightened) {
            frightenedTimeLeft -= Time.deltaTime;

            if (frightenedTimeLeft <= 0f) {
                EndFrightened();
            }

            return;
        }

        AdvanceScatterChaseClock();
    }

    private void AdvanceScatterChaseClock() {
        int phaseCount = ghostModeTimingSO.GetPhaseCount();
        if (phaseIndex >= phaseCount) return;

        phaseTimeLeft -= Time.deltaTime;
        if (phaseTimeLeft > 0f) return;

        phaseIndex++;

        if (phaseIndex >= phaseCount) {
            SetMode(GhostMode.Chase);
            return;
        }

        phaseTimeLeft = ghostModeTimingSO.GetPhaseDuration(phaseIndex);
        SetMode(phaseIndex % 2 == 0 ? GhostMode.Scatter : GhostMode.Chase);
    }

    private void EndFrightened() {
        isFrightened = false;
        Debug.Log("Frightened ended");
        OnFrightenedEnded?.Invoke(this, EventArgs.Empty);
    }

    private void SetMode(GhostMode mode) {
        if (currentGhostMode == mode) return;

        currentGhostMode = mode;
        OnModeChanged?.Invoke(this, new OnModeChangedEventArgs(mode));
    }

    public GhostMode GetCurrentMode() {
        return currentGhostMode;
    }

    public bool IsFrightened() {
        return isFrightened;
    }
}