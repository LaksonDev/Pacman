using System;
using UnityEngine;

public class GhostVisual : MonoBehaviour {

    [SerializeField] private BaseGhost ghost;
    [SerializeField] private SpriteRenderer eyesRenderer;

    // Body
    [SerializeField] private Sprite[] bodySpriteArray;
    [SerializeField, Min(0.05f)] private float bodyFrameInterval = 0.15f;
    
    // Eyes
    [SerializeField] private Sprite eyesUpSprite;
    [SerializeField] private Sprite eyesDownSprite;
    [SerializeField] private Sprite eyesLeftSprite;
    [SerializeField] private Sprite eyesRightSprite;

    private int shownBodyFrame = -1;
    private SpriteRenderer bodyRenderer;

    private void Awake() {
        bodyRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() {
        ghost.OnDirectionChanged += Ghost_OnDirectionChanged;
        
        UpdateEyes();
    }
    
    private void OnDisable() {
        ghost.OnDirectionChanged -= Ghost_OnDirectionChanged;
    }

    private void Update() {
        UpdateBodyFrame();
    }

    private void Ghost_OnDirectionChanged(object sender, EventArgs e) {
        UpdateEyes();
    }

    private void UpdateBodyFrame() {
        if (bodySpriteArray.Length == 0) return;
        if (ghost.GetCurrentDirection() == Vector2.zero) return;
        
        int frame = Mathf.FloorToInt(Time.time / bodyFrameInterval) % bodySpriteArray.Length;
        if (frame == shownBodyFrame) return;

        shownBodyFrame = frame;
        bodyRenderer.sprite = bodySpriteArray[frame];
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
