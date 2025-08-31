// Assets/Scripts/StageFlow/LoadingShellController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingShellController : MonoBehaviour
{
    [Header("Next Stage Plan")]
    public StageRunPlan plan;  // StageRunPlan を追加
    public string nextScene;

    private bool isReady = false;
    private bool isDone = false;

    [Header("Debug Options")]
    [Tooltip("起動時に自動的にReadyにしてStartNextStageを呼ぶ診断モード")]
    public bool autoProceedDebug = true;

    void Start()
    {
        if (plan == null)
        {
            Debug.LogWarning("[LoadingShellController] StageRunPlanが設定されていません。");
            nextScene = "Main";  // Fallback のシーン
        }
        else
        {
            nextScene = plan.stages[0]; // 最初のステージ名を次のシーンとして設定
        }

        Debug.Log("[LoadingShellController] Next stage set: " + nextScene);

        // ★診断モード：起動時に強制Ready→StartNextStageを呼ぶ
        if (autoProceedDebug)
        {
            SetReady();
            StartNextStage();
        }
    }

    public void SetReady()
    {
        isReady = true;
        Debug.Log("[LoadingShellController] Stage is ready.");
    }

    public void StartNextStage()
    {
        Debug.Log("[TRACE] LoadingShellController.StartNextStage called (isReady=" + isReady + ")");
        if (isReady && !isDone)
        {
            isDone = true; // 二重呼び出し防止
            SceneManager.LoadScene(nextScene);
            Debug.Log("[TRACE] LoadingShellController loading next scene: " + nextScene);
        }
    }
}
