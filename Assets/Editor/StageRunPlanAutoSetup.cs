// Assets/Editor/StageRunPlanAutoSetup.cs
using UnityEditor;
using UnityEngine;

public class StageRunPlanAutoSetup : EditorWindow
{
    [MenuItem("Tools/StageRunPlan/Auto Setup")]
    public static void ShowWindow()
    {
        GetWindow<StageRunPlanAutoSetup>("StageRunPlan Auto Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("StageRunPlanの自動設定", EditorStyles.boldLabel);

        if (GUILayout.Button("Set StageRunPlan to LoadingShellController"))
        {
            ApplyStageRunPlanToLoadingShellController();
        }
    }

    private void ApplyStageRunPlanToLoadingShellController()
    {
        var loadingShellController = GameObject.FindObjectOfType<LoadingShellController>();

        if (loadingShellController != null)
        {
            StageRunPlan stageRunPlan = AssetDatabase.LoadAssetAtPath<StageRunPlan>("Assets/StageRunPlan.asset");

            if (stageRunPlan != null)
            {
                // StageRunPlan を LoadingShellController に設定
                loadingShellController.plan = stageRunPlan;
                Debug.Log("[StageRunPlanAutoSetup] StageRunPlan を LoadingShellController に設定しました。");
            }
            else
            {
                Debug.LogWarning("[StageRunPlanAutoSetup] StageRunPlan アセットが見つかりませんでした。");
            }
        }
        else
        {
            Debug.LogWarning("[StageRunPlanAutoSetup] LoadingShellController がシーン内に存在しません。");
        }
    }
}
