// Assets/Editor/StageRunPlanDebugWindow.cs
// 目的: Unityエディタ内のウィンドウを使用して、StageRunPlanの設定を確認
using UnityEditor;
using UnityEngine;

public class StageRunPlanDebugWindow : EditorWindow
{
    [MenuItem("Tools/StageRunPlan/Check Plan")]
    public static void ShowWindow()
    {
        GetWindow<StageRunPlanDebugWindow>("StageRunPlan Check");
    }

    public StageRunPlan plan;

    private void OnGUI()
    {
        GUILayout.Label("StageRunPlan の設定確認", EditorStyles.boldLabel);

        plan = (StageRunPlan)EditorGUILayout.ObjectField("StageRunPlan", plan, typeof(StageRunPlan), false);

        if (plan == null)
        {
            GUILayout.Label("StageRunPlan が設定されていません。");
        }
        else
        {
            GUILayout.Label("設定されたステージ:");
            foreach (var stage in plan.stages)
            {
                GUILayout.Label($"- {stage}");
            }
        }

        if (GUILayout.Button("Check Plan"))
        {
            if (plan != null)
            {
                Debug.Log("[StageRunPlanDebugWindow] StageRunPlan is set correctly.");
            }
        }
    }
}
