using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSpeedController : MonoBehaviour {

    [SerializeField, Min(0.01f)] private float eatingGraceSeconds = 0.2f;
    
    private Player player;
    private WaitForSeconds eatingGraceWait;
    private Coroutine eatingGraceCoroutine;
    private bool isEating;

    private void Awake() {
        player = GetComponent<Player>();
        eatingGraceWait = new WaitForSeconds(eatingGraceSeconds);
    }

    private void OnEnable() {
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void Pellet_OnAnyPelletEaten(object sender, Pellet.OnAnyPelletEatenEventArgs e) {
        if (eatingGraceCoroutine != null) {
            StopCoroutine(eatingGraceCoroutine);
        }

        isEating = true;
        ApplySpeed();

        eatingGraceCoroutine = StartCoroutine(EatingGraceRoutine());
    }

    private void Start() {
        GameManager.Instance.OnLevelChanged += GameManager_OnLevelChanged;

        ApplySpeed();
    }

    private void OnDisable() {
        Pellet.OnAnyPelletEaten -= Pellet_OnAnyPelletEaten;

        isEating = false;
        eatingGraceCoroutine = null;
    }

    private IEnumerator EatingGraceRoutine() {
        yield return eatingGraceWait;

        isEating = false;
        eatingGraceCoroutine = null;
        ApplySpeed();
    }

    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnLevelChanged -= GameManager_OnLevelChanged;
        }
    }

    private void GameManager_OnLevelChanged(object sender, EventArgs e) {
        ApplySpeed();
    }

    private void ApplySpeed() {
        LevelTier tier = GameManager.Instance.GetCurrentTier();

        float multiplier = isEating ? tier.GetPacmanEatingDotsSpeedMultiplier() : tier.GetPacmanNormalSpeedMultiplier();
        
        player.SetSpeedMultiplier(multiplier);
    }
}
