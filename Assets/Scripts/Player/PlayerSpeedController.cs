using System;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSpeedController : MonoBehaviour {

    private Player player;

    private void Awake() {
        player = GetComponent<Player>();
    }

    private void Start() {
        GameManager.Instance.OnLevelChanged += GameManager_OnLevelChanged;
        
        ApplyLevelSpeed();
    }

    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnLevelChanged -= GameManager_OnLevelChanged;
        }
    }

    private void GameManager_OnLevelChanged(object sender, EventArgs e) {
        ApplyLevelSpeed();
    }

    private void ApplyLevelSpeed() {
        float multiplier = GameManager.Instance.GetCurrentTier().GetPacmanNormalSpeedMultiplier();
        player.SetSpeedMultiplier(multiplier);
    }
}
