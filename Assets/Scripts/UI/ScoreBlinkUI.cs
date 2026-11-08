using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreBlinkUI : MonoBehaviour {
    
    [SerializeField] private TextMeshProUGUI UPTextMesh;
    [SerializeField, Min(0.05f)] private float blinkInterval = 0.25f;

    private Coroutine blinkRoutine;

    private void OnEnable() {
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }

        StopBlinking();
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        StopBlinking();
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        StopBlinking();
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine() {
        float startTime = Time.time;

        while (true) {
            int phase = Mathf.FloorToInt((Time.time - startTime) / blinkInterval);
            UPTextMesh.enabled = phase % 2 != 0;

            yield return null;
        }
    }

    private void StopBlinking() {
        if (blinkRoutine != null) {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
        
        if (UPTextMesh != null) {
            UPTextMesh.enabled = true;
        }
    }
}
