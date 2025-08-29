using UnityEngine;

[System.Serializable]
public struct AbilityMask
{
    public bool canRun;
    public bool canJump;
    public bool allowNearFarToggle;
    [Range(0.2f, 3f)] public float maxSpeedMultiplier;

    public static AbilityMask Default => new AbilityMask
    {
        canRun = true,
        canJump = true,
        allowNearFarToggle = true,
        maxSpeedMultiplier = 1f
    };
}

public enum CameraMode
{
    FixedFar,
    FixedNear,
    OrbitThirdPerson,
    FirstPerson,
    CinematicFollow
}
