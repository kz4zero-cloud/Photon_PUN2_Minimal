// Assets/Scripts/StageFlow/StageRunStarter.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StageRunStarter : MonoBehaviour
{
    [Header("Run Plan")]
    [Tooltip("今回の連戦で回すステージ順を定義したアセット")]
    public StageRunPlan plan;

    [Header("Routing")]
    [Tooltip("中継シーン名（通常は LoadingShell）")]
    public string loadingShellScene = "LoadingShell";

    [Header("Hotkey (任意)")]
    public bool enableHotkey = true;
    public KeyCode hotkey = KeyCode.P; // PでPlayのイメージ

    public void StartRun()
    {
        Debug.Log("[TRACE] StageRunStarter.StartRun called");

        if (plan == null)
        {
            Debug.LogWarning("[StageRunStarter] plan が未設定です。StageRunPlan を割り当ててください。");
            return;
        }
        StageRuntime.StartPlan(plan);
        Debug.Log("[TRACE] StageRunStarter loading shell=" + loadingShellScene);
        SceneManager.LoadScene(loadingShellScene, LoadSceneMode.Single);
    }

    private void Update()
    {
        if (enableHotkey && Input.GetKeyDown(hotkey))
        {
            StartRun();
        }
    }
}
