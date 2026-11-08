using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class WallsFlashVisual : MonoBehaviour {
    
    private static readonly int FLASH_AMOUNT = Shader.PropertyToID("_FlashAmount");

    [SerializeField] private TilemapRenderer tilemapRenderer;
    [SerializeField, Min(0.05f)] private float flashInterval = 0.25f;

    private Material wallsMaterial;
    private Coroutine flashRoutine;

    private void Awake() {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        wallsMaterial = tilemapRenderer.material;
    }

    private void OnEnable() {
        if (LevelManager.Instance == null) return;
        
        LevelManager.Instance.OnLevelCompleted += LevelManager_OnLevelCompleted;
    }

    private void OnDisable() {
        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        }

        StopFlashing();
    }

    private void OnDestroy() {
        if (wallsMaterial != null) {
            Destroy(wallsMaterial);
        }
    }

    private void LevelManager_OnLevelCompleted(object sender, LevelManager.OnLevelCompletedEventArgs e) {
        StopFlashing();
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine() {
        float startTime = Time.time;

        while (true) {
            int phase = Mathf.FloorToInt((Time.time - startTime) / flashInterval);
            SetFlashAmount(phase % 2 == 0 ? 1f : 0f);

            yield return null;
        }
    }

    private void StopFlashing() {
        if (flashRoutine != null) {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        
        SetFlashAmount(0f);
    }

    private void SetFlashAmount(float amount) {
        if (wallsMaterial == null) return;
        
        wallsMaterial.SetFloat(FLASH_AMOUNT, amount);
    }
}
