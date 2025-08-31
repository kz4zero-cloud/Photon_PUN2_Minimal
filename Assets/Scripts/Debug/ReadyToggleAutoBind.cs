using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(Toggle))]
public class ReadyToggleAutoBind : MonoBehaviour
{
    [Tooltip("未指定ならシーン内の PunStagingFlow を自動検出")]
    public PunStagingFlow flow;

    void Awake()
    {
        var t = GetComponent<Toggle>();

        // MasterクライアントはReadyトグルを使わない → 非表示にする
        if (PhotonNetwork.IsMasterClient)
        {
            t.gameObject.SetActive(false);
            Debug.Log("[ReadyToggleAutoBind] Master detected → Toggle hidden");
            return;
        }

        if (!flow) flow = FindObjectOfType<PunStagingFlow>();
        if (!flow)
        {
            NetLog.Report("AutoBind", "PunStagingFlow not found");
            return;
        }

        // Readyトグル変更時に PunStagingFlow へ伝える
        t.onValueChanged.AddListener((bool v) => {
            flow.SetReadyFromUI(v);
            NetLog.Report("AutoBindDispatch", $"ReadyToggle -> {v}");
        });

        // 起動時はOFFに揃える（UI初期値の揺れ対策）
        if (t.isOn)
        {
            t.isOn = false;
            flow.SetReadyFromUI(false);
            NetLog.Report("AutoBindInit", "Force OFF at start");
        }
        else
        {
            NetLog.Report("AutoBindInit", "Start OFF");
        }
    }
}
