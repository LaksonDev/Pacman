using System.Collections;
using UnityEngine;

public class PowerPelletBlinking : MonoBehaviour {
    
    private float interval = 0.2f;
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        StartCoroutine(Blink());
    }

    private IEnumerator Blink() {
        while (true) {
            yield return new WaitForSeconds(interval);
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }
    }
}
