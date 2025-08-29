using UnityEngine;

[CreateAssetMenu(fileName = "MinigameDef", menuName = "Goshuyo/Minigame Def")]
public class MinigameDef : ScriptableObject
{
    public string key;
    public string displayName;
    public bool isCoop;

    public enum LoadType { SceneAdditive, Prefab }
    public LoadType loadType = LoadType.Prefab;
    public string sceneName;
    public GameObject prefab;

    public int countdownSec = 5;
    public int timeLimitSec = 90;

    public AbilityMask abilities = AbilityMask.Default;
    public CameraMode cameraMode = CameraMode.FixedFar;

    public bool supportsBots = true;
    public int botDefault = 3;
    public Vector2Int botMinMax = new Vector2Int(0, 6);

    public string scoreRuleId = "default";
    [TextArea] public string uiHint;
}

[CreateAssetMenu(fileName = "MinigameRegistry", menuName = "Goshuyo/Minigame Registry")]
public class MinigameRegistry : ScriptableObject
{
    public MinigameDef[] order; // ÉâÉEÉìÉhèá
}
