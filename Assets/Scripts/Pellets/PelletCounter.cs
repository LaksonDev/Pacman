using System;
using UnityEngine;

public class PelletCounter : MonoBehaviour {

    public event EventHandler OnAllPelletsEaten;

    private int remainingPellets;

    private void OnEnable() {
        Pellet.OnAnyPelletSpawned += Pellet_OnAnyPelletSpawned;
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void OnDisable() {
        Pellet.OnAnyPelletSpawned += Pellet_OnAnyPelletSpawned;
        Pellet.OnAnyPelletEaten += Pellet_OnAnyPelletEaten;
    }

    private void Pellet_OnAnyPelletEaten(object sender, Pellet.OnAnyPelletEatenEventArgs e) {
        if (remainingPellets <= 0) return;

        remainingPellets--;
        Debug.Log($"Remaining pellets: {remainingPellets}"); // temporary

        if (remainingPellets == 0) {
            Debug.Log("ALL PELLETS EATEN");
            OnAllPelletsEaten?.Invoke(this,EventArgs.Empty);
        }
    }

    private void Pellet_OnAnyPelletSpawned(object sender, EventArgs e) {
        remainingPellets++;
    }

    public void ResetCount() {
        remainingPellets = 0;
    }

    public int GetRemainingPelletCount() {
        return remainingPellets;
    }
}
