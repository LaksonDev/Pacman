using System;
using UnityEngine;

[Serializable]
public struct GhostReleaseEntry {

    [SerializeField] private BaseGhost ghost;
    [SerializeField, Min(0)] private int dotLimit;

    public BaseGhost GetGhost() {
        return ghost;
    }

    public int GetDotLimit() {
        return dotLimit;
    }
}