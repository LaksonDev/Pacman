using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ScoreManager : MonoBehaviour {
    
    public static ScoreManager Instance { get; private set; }

    public event EventHandler OnScoreChanged;

    private int score;

    private void Awake() {
        Instance = this;
    }

    private void OnEnable() {
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void OnDisable() {
        Pellet.OnAnyPelletEaten -= Pellet_OnAnyPelletEaten;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }
    
    public void ResetScore() {
        score = 0;
        OnScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Pellet_OnAnyPelletEaten(object sender, Pellet.OnAnyPelletEatenEventArgs e) {
        AddPoints(e.GetPelletSO().GetPoints());
    }

    private void AddPoints(int points) {
        if (points <= 0) return;

        score += points;
        OnScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetScore() {
        return score;
    }
}
