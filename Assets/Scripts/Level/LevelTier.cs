using System;
using UnityEngine;

[Serializable]
public struct LevelTier {

    private const float MAX_SPEED_PERFNT = 100f;
    
    [SerializeField, Min(1)] private int fromLevel;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float pacmanNormalSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float pacmanEatingDotsSpeedPercent;

    public int GetFromLevel() {
        return fromLevel;
    }

    public float GetPacmanNormalSpeedMultiplier() {
        return PercentToMultiplier(pacmanNormalSpeedPercent);
    }

    public float GetPacmanEatingDotsSpeedMultiplier() {
        return PercentToMultiplier(pacmanEatingDotsSpeedPercent);
    }

    private static float PercentToMultiplier(float percent) {
        return percent / 100f;
    }
}
