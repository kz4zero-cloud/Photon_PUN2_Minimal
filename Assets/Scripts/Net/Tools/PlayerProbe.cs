using Photon.Pun;
using UnityEngine;

/// <summary>
/// 生成直後のプレイヤー個体に自動で付き、数フレームにわたり状態をログする。
/// - PunPlayer/PhotonView の有無
/// - IsMine / ViewID
/// - 一定時間後も PunPlayer が見つからない場合は強い警告
/// </summary>
public class PlayerProbe : MonoBehaviour
{
    void OnEnable()
    {
        Invoke(nameof(DumpNow), 0.02f);
        Invoke(nameof(DumpNow), 0.20f);
        Invoke(nameof(FinalCheck), 0.60f);
    }

    void DumpNow()
    {
        var pv = GetComponent<PhotonView>();
        var pp = GetComponent<PunPlayer>();
        Debug.Log($"[PlayerProbe] name={name} active={gameObject.activeInHierarchy} " +
                  $"PV={(pv ? "OK" : "NONE")} IsMine={(pv ? pv.IsMine : false)} ViewID={(pv ? pv.ViewID : 0)} " +
                  $"PunPlayer={(pp ? "OK" : "NONE")}");
    }

    void FinalCheck()
    {
        var pv = GetComponent<PhotonView>();
        var pp = GetComponent<PunPlayer>();
        if (pp == null)
        {
            Debug.LogError("[PlayerProbe] PunPlayer component STILL missing on spawned object. " +
                           "Prefab差異/スクリプト失敗/Guid破損の可能性。Player.prefab を確認してください。");
        }
    }
}
