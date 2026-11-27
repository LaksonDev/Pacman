using System;
using UnityEngine;

[Serializable]
public struct LevelTier {

    private const float MAX_SPEED_PERCENT = 100f;

    [SerializeField, Min(1)] private int fromLevel;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float pacmanNormalSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float pacmanEatingDotsSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float ghostNormalSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float ghostTunnelSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float ghostFrightenedSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERCENT)] private float ghostEatenSpeedPercent;
    [SerializeField, Min(0f)] private float frightenedDurationSeconds;

    public int GetFromLevel() {
        return fromLevel;
    }

    public float GetPacmanNormalSpeedMultiplier() {
        return PercentToMultiplier(pacmanNormalSpeedPercent);
    }

    public float GetPacmanEatingDotsSpeedMultiplier() {
        return PercentToMultiplier(pacmanEatingDotsSpeedPercent);
    }

    public float GetGhostNormalSpeedMultiplier() {
        return PercentToMultiplier(ghostNormalSpeedPercent);
    }

    public float GetGhostTunnelSpeedMultiplier() {
        return PercentToMultiplier(ghostTunnelSpeedPercent);
    }

    public float GetGhostFrightenedSpeedMultiplier() {
        return PercentToMultiplier(ghostFrightenedSpeedPercent);
    }

    public float GetGhostEatenSpeedMultiplier() {
        return PercentToMultiplier(ghostEatenSpeedPercent);
    }

    public float GetFrightenedDuration() {
        return frightenedDurationSeconds;
    }

    private static float PercentToMultiplier(float percent) {
        return percent / 100f;
    }
}