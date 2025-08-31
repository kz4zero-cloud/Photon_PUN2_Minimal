// Assets/Scripts/StageFlow/StageSelectUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// ステージ選択画面の UI 制御
/// Master は複数ステージを選んで Confirm で確定。
/// 選択されたプランは PunStagingFlow に送られ、Room 全体に同期される。
/// </summary>
public class StageSelectUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private StageList stageList;          // 利用可能ステージ一覧 (ScriptableObject)
    [SerializeField] private Button stageButtonPrefab;     // ステージボタンのプレハブ
    [SerializeField] private Transform stageButtonParent;  // ボタン配置先
    [SerializeField] private TextMeshProUGUI selectedListText; // 選択中リスト表示
    [SerializeField] private Button confirmButton;         // 決定ボタン

    private List<string> selectedStages = new List<string>();
    private PunStagingFlow flow;

    private void Awake()
    {
        flow = FindObjectOfType<PunStagingFlow>();
        if (flow == null)
        {
            Debug.LogError("[StageSelectUI] PunStagingFlow がシーンに存在しません");
        }
    }

    private void Start()
    {
        // ボタンを動的に生成
        foreach (var stage in stageList.stageNames)
        {
            var btn = Instantiate(stageButtonPrefab, stageButtonParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = stage;
            string stageName = stage;
            btn.onClick.AddListener(() => ToggleStage(stageName));
        }

        // Confirmボタン
        confirmButton.onClick.AddListener(OnConfirm);
        RefreshUI();
    }

    // ステージ選択のON/OFF
    private void ToggleStage(string stageName)
    {
        if (selectedStages.Contains(stageName))
        {
            selectedStages.Remove(stageName);
        }
        else
        {
            selectedStages.Add(stageName);
        }
        RefreshUI();
    }

    // 選択中リストを更新
    private void RefreshUI()
    {
        if (selectedListText != null)
        {
            if (selectedStages.Count == 0)
            {
                selectedListText.text = "選択中: なし";
            }
            else
            {
                selectedListText.text = "選択中:\n" + string.Join("\n", selectedStages);
            }
        }
    }

    // Confirmボタン押下
    private void OnConfirm()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[StageSelectUI] ConfirmはMasterのみが有効です");
            return;
        }

        if (selectedStages.Count == 0)
        {
            Debug.LogWarning("[StageSelectUI] ステージが選択されていません");
            return;
        }

        flow.SetPlanOnly(selectedStages);
        Debug.Log("[StageSelectUI] Plan confirmed and sent to PunStagingFlow: " + string.Join(",", selectedStages));
    }
}
