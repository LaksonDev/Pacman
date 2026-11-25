using System;
using UnityEngine;

[Serializable]
public struct LevelTier {

    private const float MAX_SPEED_PERFNT = 100f;

    [SerializeField, Min(1)] private int fromLevel;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float pacmanNormalSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float pacmanEatingDotsSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float ghostNormalSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float ghostTunnelSpeedPercent;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float ghostFrightenedSpeedPercent;
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

    public float GetFrightenedDuration() {
        return frightenedDurationSeconds;
    }

    private static float PercentToMultiplier(float percent) {
        return percent / 100f;
    }
}