using System.Collections;
using UnityEngine;

public class GhostHouseReleaser : MonoBehaviour {

    [SerializeField] private GhostReleaseEntry[] releaseQueue;

    private int nextIndex;
    private int dotsEatenForNext;
    private float secondsSinceLastDot;
    private bool isTimerRunning;
    private float noDotTimeoutSeconds = 4f;

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
            LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        }

        Pellet.OnAnyPelletEaten -= Pellet_OnAnyPelletEaten;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        nextIndex = 0;
        dotsEatenForNext = 0;
        secondsSinceLastDot = 0f;
        isTimerRunning = true;

        StartCoroutine(ReleaseAfterGhostsResetRoutine());
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        isTimerRunning = false;
    }

    private IEnumerator ReleaseAfterGhostsResetRoutine() {
        yield return null;

        ReleaseReadyGhosts();
    }

    private void Update() {
        if (!isTimerRunning || nextIndex >= releaseQueue.Length) return;

        secondsSinceLastDot += Time.deltaTime;

        if (secondsSinceLastDot < noDotTimeoutSeconds) return;

        ReleaseNextGhost();
        ReleaseReadyGhosts();
    }

    private void Pellet_OnAnyPelletEaten(object sender, Pellet.OnAnyPelletEatenEventArgs e) {
        if (nextIndex >= releaseQueue.Length) return;

        secondsSinceLastDot = 0f;
        dotsEatenForNext++;
        ReleaseReadyGhosts();
    }

    private void ReleaseReadyGhosts() {
        while (nextIndex < releaseQueue.Length && dotsEatenForNext >= releaseQueue[nextIndex].GetDotLimit()) {
            ReleaseNextGhost();
        }
    }

    private void ReleaseNextGhost() {
        releaseQueue[nextIndex].GetGhost().ReleaseFromHouse();

        nextIndex++;
        dotsEatenForNext = 0;
        secondsSinceLastDot = 0f;
    }
}