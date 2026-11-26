using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour {

    [SerializeField] private Transform iconsRoot;
    [SerializeField] private GameObject lifeIconPrefab;

    private readonly List<GameObject> lifeIcons = new List<GameObject>();

    private void Start() {
        Player.Instance.OnLifeLost += Player_OnLivesChanged;
        Player.Instance.OnGameOver += Player_OnLivesChanged;
        LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;

        RefreshIcons();
    }

    private void OnDestroy() {
        if (Player.Instance != null) {
            Player.Instance.OnLifeLost -= Player_OnLivesChanged;
            Player.Instance.OnGameOver -= Player_OnLivesChanged;
        }

        if (LevelManager.Instance != null) {
            LevelManager.Instance.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        }
    }

    private void Player_OnLivesChanged(object sender, EventArgs e) {
        RefreshIcons();
    }

    private void LevelManager_OnLevelLoaded(object sender, LevelManager.OnLevelLoadedEventArgs e) {
        RefreshIcons();
    }

    private void RefreshIcons() {
        int lives = Player.Instance.GetLives();

        while (lifeIcons.Count < lives) {
            lifeIcons.Add(Instantiate(lifeIconPrefab, iconsRoot));
        }

        while (lifeIcons.Count > lives) {
            int lastIndex = lifeIcons.Count - 1;
            Destroy(lifeIcons[lastIndex].gameObject);
            lifeIcons.RemoveAt(lastIndex);
        }
    }
}