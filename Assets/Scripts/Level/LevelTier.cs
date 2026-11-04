using System;
using UnityEngine;

[Serializable]
public class LevelTier : MonoBehaviour {

    [SerializeField, Min(1)] private int fromLevel;
    [SerializeField, Range(0f, 200f)] private float pacmanNormalSpeedPercent;

    public int GetFromLevel() {
        return fromLevel;
    }

    public float GetPacmanNormalSpeedMultiplier() {
        return pacmanNormalSpeedPercent / 100f;
    }
}
