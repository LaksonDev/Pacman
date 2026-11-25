using System;
using UnityEngine;

public class GhostVisual : MonoBehaviour {

    [SerializeField] private BaseGhost ghost;
    [SerializeField] private SpriteRenderer eyesRenderer;

    [Header("Body")]
    [SerializeField] private Sprite[] bodySpriteArray;
    [SerializeField, Min(0.05f)] private float bodyFrameInterval = 0.15f;

    [Header("Eyes")]
    [SerializeField] private Sprite eyesUpSprite;
    [SerializeField] private Sprite eyesDownSprite;
    [SerializeField] private Sprite eyesLeftSprite;
    [SerializeField] private Sprite eyesRightSprite;

    [Header("Frightened")]
    [SerializeField] private Sprite[] frightenedBlueSpriteArray;
    [SerializeField] private Sprite[] frightenedWhiteSpriteArray;
    [SerializeField, Min(0f)] private float flashStartSeconds = 2f;
    [SerializeField, Min(0.05f)] private float flashIntervalSeconds = 0.2f;
    
    private SpriteRenderer bodyRenderer;
    private Color normalBodyColor;

    private void Awake() {
        bodyRenderer = GetComponent<SpriteRenderer>();
        normalBodyColor = bodyRenderer.color;
    }

    private void OnEnable() {
        ghost.OnDirectionChanged += Ghost_OnDirectionChanged;
        ghost.OnFrightenedChanged += Ghost_OnFrightenedChanged;
        ghost.OnEatenChanged += Ghost_OnEatenChanged;

        RefreshAppearance();
        UpdateEyes();
    }

    private void OnDisable() {
        ghost.OnDirectionChanged -= Ghost_OnDirectionChanged;
        ghost.OnFrightenedChanged -= Ghost_OnFrightenedChanged;
        ghost.OnEatenChanged -= Ghost_OnEatenChanged;
    }

    private void Update() {
        UpdateBodySprite();
    }
    
    private void Ghost_OnEatenChanged(object sender, EventArgs e) {
        RefreshAppearance();
        UpdateEyes();
    }

    private void Ghost_OnDirectionChanged(object sender, EventArgs e) {
        UpdateEyes();
    }

    private void Ghost_OnFrightenedChanged(object sender, EventArgs e) {
        RefreshAppearance();
        UpdateEyes();
    }

    private void RefreshAppearance() {
        bool isEaten = ghost.IsEaten();
        bool isFrightened = ghost.IsFrightened();

        bodyRenderer.enabled = !isEaten;
        eyesRenderer.enabled = isEaten || !isFrightened;
        bodyRenderer.color = isFrightened ? Color.white : normalBodyColor;

        if (isEaten) return;

        UpdateBodySprite();
    }

    private void UpdateBodySprite() {
        if (ghost.IsEaten()) return;
        
        Sprite[] spriteArray = GetActiveSpriteArray();
        if (spriteArray.Length == 0) return;

        int frame = ghost.GetCurrentDirection() == Vector2.zero
            ? 0
            : Mathf.FloorToInt(Time.time / bodyFrameInterval) % spriteArray.Length;
        Sprite sprite = spriteArray[frame];

        if (bodyRenderer.sprite == sprite) return;

        bodyRenderer.sprite = sprite;
    }

    private Sprite[] GetActiveSpriteArray() {
        if (!ghost.IsFrightened()) return bodySpriteArray;

        return IsFlashingWhite() ? frightenedWhiteSpriteArray : frightenedBlueSpriteArray;
    }

    private bool IsFlashingWhite() {
        if (GhostModeScheduler.Instance == null) return false;

        float timeLeft = GhostModeScheduler.Instance.GetFrightenedTimeLeft();
        if (timeLeft > flashStartSeconds) return false;

        return Mathf.FloorToInt(timeLeft / flashIntervalSeconds) % 2 == 1;
    }

    private void UpdateEyes() {
        eyesRenderer.sprite = GetEyesSprite(ghost.GetCurrentDirection());
    }

    private Sprite GetEyesSprite(Vector2 direction) {
        if (direction == Vector2.up) return eyesUpSprite;
        if (direction == Vector2.down) return eyesDownSprite;
        if (direction == Vector2.right) return eyesRightSprite;

        return eyesLeftSprite;
    }
}