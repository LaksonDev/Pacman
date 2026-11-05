using System;
using UnityEngine;

[Serializable]
public struct LevelTier {

    private const float MAX_SPEED_PERFNT = 100f;
    
    [SerializeField, Min(1)] private byte fromLevel;
    [SerializeField, Range(0f, MAX_SPEED_PERFNT)] private float pacmanNormalSpeedPercent;

    public byte GetFromLevel() {
        return fromLevel;
    }

    public float GetPacmanNormalSpeedMultiplier() {
        return pacmanNormalSpeedPercent / 100f;
    }
}
