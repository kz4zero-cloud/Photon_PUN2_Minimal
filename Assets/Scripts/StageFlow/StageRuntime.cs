// Assets/Scripts/StageFlow/StageRuntime.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public sealed class StageRuntime : MonoBehaviour
{
    private static StageRunPlan s_plan;
    private static int s_index = -1;

    /// <summary>
    /// プランを開始する
    /// </summary>
    public static void StartPlan(StageRunPlan plan)
    {
        Debug.Log("[TRACE] StageRuntime.StartPlan called");
        s_plan = plan;
        s_index = 0;
    }

    /// <summary>
    /// 次のステージへ進む
    /// </summary>
    public static void LoadNextStage()
    {
        Debug.Log("[TRACE] StageRuntime.LoadNextStage called (index=" + s_index + ")");

        if (s_plan == null || s_plan.stages == null || s_plan.stages.Count == 0)
        {
            Debug.LogWarning("[StageRuntime] plan が未設定か空です。Main に戻ります。");
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
            return;
        }

        if (s_index < 0 || s_index >= s_plan.stages.Count)
        {
            Debug.Log("[StageRuntime] すべてのステージが終了しました。Main に戻ります。");
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
            return;
        }

        string nextStage = s_plan.stages[s_index];
        Debug.Log("[TRACE] StageRuntime.LoadNextStage -> loading: " + nextStage);

        SceneManager.LoadScene(nextStage, LoadSceneMode.Single);
    }

    /// <summary>
    /// 現在のステージを実行する
    /// </summary>
    public static void RunCurrentStage()
    {
        Debug.Log("[TRACE] StageRuntime.RunCurrentStage called (index=" + s_index + ")");
        if (s_plan == null || s_plan.stages == null || s_plan.stages.Count == 0)
        {
            Debug.LogWarning("[StageRuntime] plan が未設定か空です。Main に戻ります。");
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
            return;
        }

        if (s_index < 0 || s_index >= s_plan.stages.Count)
        {
            Debug.Log("[StageRuntime] index が範囲外です。Main に戻ります。");
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
            return;
        }

        string currentStage = s_plan.stages[s_index];
        Debug.Log("[TRACE] StageRuntime.RunCurrentStage -> loading: " + currentStage);

        SceneManager.LoadScene(currentStage, LoadSceneMode.Single);
    }

    /// <summary>
    /// ステージ完了時に呼ばれる
    /// </summary>
    public static void OnStageComplete()
    {
        Debug.Log("[TRACE] StageRuntime.OnStageComplete called (index=" + s_index + ")");
        s_index++;
        LoadNextStage();
    }
}
