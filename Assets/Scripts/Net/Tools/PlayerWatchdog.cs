using Photon.Pun;
using UnityEngine;

/// <summary>
/// 自キャラ/他キャラのライフサイクルを詳細ログに出す監視用スクリプト。
/// </summary>
public class PlayerWatch : MonoBehaviour
{
    private PhotonView _pv;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        Log("Awake");
    }

    private void OnEnable()
    {
        Log("OnEnable");
    }

    private void Start()
    {
        Log("Start");
    }

    private void OnDisable()
    {
        Log("OnDisable");
    }

    private void OnDestroy()
    {
        Log("OnDestroy");
        // 万一自分の本体を消すコードがどこかにあればここで痕跡が分かる
        if (_pv && _pv.IsMine)
        {
            if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.TagObject as GameObject == gameObject)
            {
                PhotonNetwork.LocalPlayer.TagObject = null;
                Debug.Log("[PlayerWatch] Cleared TagObject because the local player object is being destroyed.");
            }
        }
    }

    private void Log(string hook)
    {
        string owner = _pv ? (_pv.IsMine ? "MINE" : $"Remote({(_pv.Owner != null ? _pv.Owner.NickName : "null")})") : "no PV";
        Debug.Log($"[PlayerWatch] {hook}  name={gameObject.name}  owner={owner}  active={gameObject.activeInHierarchy}");
    }
}
