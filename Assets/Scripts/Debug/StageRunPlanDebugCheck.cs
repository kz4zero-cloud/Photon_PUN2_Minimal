// Assets/Scripts/Debug/StageRunPlanDebugCheck.cs
// 目的: `StageRunPlan` が正しく設定されているか、ステージ遷移が正常に行われるかを確認
using UnityEngine;

public class StageRunPlanDebugCheck : MonoBehaviour
{
    public StageRunPlan plan;

    void Start()
    {
        if (plan == null)
        {
            Debug.LogError("[StageRunPlanDebugCheck] StageRunPlan is not assigned!");
        }
        else
        {
            Debug.Log("[StageRunPlanDebugCheck] StageRunPlan is set correctly.");
            foreach (var stage in plan.stages)
            {
                Debug.Log($"[StageRunPlanDebugCheck] Stage: {stage}");
            }
        }

        // 現在のステージ進行をチェック
        Debug.Log("[StageRunPlanDebugCheck] Checking stage progression...");
        CheckStageProgression();
    }

    void CheckStageProgression()
    {
        if (plan != null && plan.stages.Count > 0)
        {
            Debug.Log("[StageRunPlanDebugCheck] Stage 1: " + plan.stages[0]);
            Debug.Log("[StageRunPlanDebugCheck] Stage 2: " + plan.stages[1]);
        }
        else
        {
            Debug.LogWarning("[StageRunPlanDebugCheck] Plan stages are not set correctly.");
        }
    }
}
