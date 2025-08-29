using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// マスターだけに操作させたいUI用のゲート。
/// ・非マスターには隠す or 操作不能にする
/// ・マスター移譲時にも自動で状態更新
/// ・Consoleは使わず NetLog で記録（NetLog は既存前提）
///
/// 使い方:
///   1) Mainシーンの「ゲーム選択」ボタン(やその親)に Add Component
///   2) 非マスター時の挙動を選ぶ（Hide / Disable）
/// </summary>
[DisallowMultipleComponent]
public class MasterOnlyUI : MonoBehaviourPunCallbacks
{
    public enum NonMasterBehavior
    {
        HideGameObject,     // 非マスターにはGameObjectをまるごと非表示
        DisableInteractable // 非マスターには操作不能(interactable=false・半透明)
    }

    [Header("Target")]
    [Tooltip("未指定ならこのGameObjectに適用")]
    public GameObject target;

    [Header("Behavior")]
    public NonMasterBehavior behavior = NonMasterBehavior.HideGameObject;

    [Tooltip("Disable時に操作不能とするSelectable群（未指定なら子階層から自動収集）")]
    public Selectable[] selectables;

    [Tooltip("Disable時にCanvasGroupを追加して視覚的に半透明へ")]
    public bool addCanvasGroup = true;

    [Range(0f, 1f)]
    public float disabledAlpha = 0.5f;

    void Awake()
    {
        if (!target) target = gameObject;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Apply("Enable");
    }

    public override void OnJoinedRoom() { Apply("OnJoinedRoom"); }
    public override void OnLeftRoom() { Apply("OnLeftRoom"); }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Apply("OnMasterSwitched");
    }

    void Apply(string tag)
    {
        // InRoom 前は確定できないので保留
        if (!PhotonNetwork.InRoom)
        {
            NetLog.Report("MasterUI", $"Defer {tag} InRoom:false");
            return;
        }

        bool isMaster = PhotonNetwork.IsMasterClient;

        if (behavior == NonMasterBehavior.HideGameObject)
        {
            if (target.activeSelf != isMaster) target.SetActive(isMaster);
            NetLog.Report("MasterUI", isMaster ? "Visible(Master)" : "Hidden(NonMaster)");
            return;
        }

        // DisableInteractable の場合
        if (selectables == null || selectables.Length == 0)
            selectables = target.GetComponentsInChildren<Selectable>(true);

        foreach (var s in selectables)
            if (s) s.interactable = isMaster;

        if (addCanvasGroup)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (!cg) cg = target.AddComponent<CanvasGroup>();
            cg.alpha = isMaster ? 1f : disabledAlpha;
            cg.interactable = isMaster;
            cg.blocksRaycasts = isMaster;
        }

        NetLog.Report("MasterUI", isMaster ? "Enabled(Master)" : "Disabled(NonMaster)");
    }
}
