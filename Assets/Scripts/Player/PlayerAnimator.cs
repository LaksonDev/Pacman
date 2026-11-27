using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour {

    private static readonly int IS_WALKING = Animator.StringToHash("IsWalking");
    private static readonly int IS_DEAD = Animator.StringToHash("IsDead");

    [SerializeField] private Player player;

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        player.OnWalkingChanged += Player_OnWalkingChanged;
        player.OnDeathAnimationStarted += Player_OnDeathAnimationStarted;
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;

        animator.SetBool(IS_WALKING, player.GetPlayerIsWalking());
    }

    private void OnDisable() {
        player.OnWalkingChanged -= Player_OnWalkingChanged;
        player.OnDeathAnimationStarted -= Player_OnDeathAnimationStarted;

        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }
    }

    private void Player_OnWalkingChanged(object sender, EventArgs e) {
        animator.SetBool(IS_WALKING, player.GetPlayerIsWalking());
    }

    private void Player_OnDeathAnimationStarted(object sender, EventArgs e) {
        transform.rotation = Quaternion.identity;        
        animator.SetBool(IS_DEAD, true);
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        transform.localRotation = Quaternion.identity;
        animator.SetBool(IS_DEAD, false);
    }
}