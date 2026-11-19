using System.Collections;
using UnityEngine;

public class GhostHouseReleaser : MonoBehaviour {

    [SerializeField] private GhostReleaseEntry[] releaseQueue;

    private int nextIndex;
    private int dotsEatenForNext;

    private void OnEnable() {
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }

        Pellet.OnAnyPelletEaten -= Pellet_OnAnyPelletEaten;
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        nextIndex = 0;
        dotsEatenForNext = 0;

        StartCoroutine(ReleaseAfterGhostsResetRoutine());
    }

    private IEnumerator ReleaseAfterGhostsResetRoutine() {
        yield return null;

        ReleaseReadyGhosts();
    }

    private void Pellet_OnAnyPelletEaten(object sender, Pellet.OnAnyPelletEatenEventArgs e) {
        if (nextIndex >= releaseQueue.Length) return;

        dotsEatenForNext++;
        ReleaseReadyGhosts();
    }

    private void ReleaseReadyGhosts() {
        while (nextIndex < releaseQueue.Length && dotsEatenForNext >= releaseQueue[nextIndex].GetDotLimit()) {
            releaseQueue[nextIndex].GetGhost().ReleaseFromHouse();

            nextIndex++;
            dotsEatenForNext = 0;
        }
    }
}