// Assets/Scripts/StageFlow/StageRunPlanCheck.cs
// 目的: StageRunPlan が正しく設定されているかを確認するためのコード。
using UnityEngine;

public class StageRunPlanCheck : MonoBehaviour
{
    public StageRunPlan plan;

    void Start()
    {
        if (plan == null)
        {
            Debug.LogError("[StageRunPlanCheck] StageRunPlan is not assigned!");
        }
        else
        {
            Debug.Log("[StageRunPlanCheck] StageRunPlan is set correctly.");
            foreach (var stage in plan.stages)
            {
                Debug.Log($"[StageRunPlanCheck] Stage: {stage}");
            }
        }

        // 現在のステージ進行をチェック
        Debug.Log("[StageRunPlanCheck] Checking stage progression...");
        CheckStageProgression();
    }

    void CheckStageProgression()
    {
        if (plan != null && plan.stages.Count > 0)
        {
            Debug.Log("[StageRunPlanCheck] Stage 1: " + plan.stages[0]);
            Debug.Log("[StageRunPlanCheck] Stage 2: " + plan.stages[1]);
        }
        else
        {
            Debug.LogWarning("[StageRunPlanCheck] Plan stages are not set correctly.");
        }
    }
}
