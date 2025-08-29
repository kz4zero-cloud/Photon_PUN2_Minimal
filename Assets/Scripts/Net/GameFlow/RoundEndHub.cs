using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Net/GameFlow/Round End Hub")]
public class RoundEndHub : MonoBehaviourPun
{
    public static RoundEndHub Instance;

    void Awake() { Instance = this; }

    /// <summary>どこからでも呼べる「ラウンド終了」エントリ</summary>
    public static void EndRound(string reason)
    {
        if (Instance != null) { Instance.EndNow(reason); }
        else { NetLog.Report("RoundEnd", $"HubMissing reason:{reason}"); }
    }

    void EndNow(string reason)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            NetLog.Report("RoundEnd", $"Master -> Main ({reason})");
            Net.Tools.SceneLoadGate.LoadLevelIfNeeded("Main");
        }
        else
        {
            NetLog.Report("RoundEnd", $"Request to Master ({reason})");
            photonView.RPC(nameof(RpcAskMasterEnd), RpcTarget.MasterClient, reason, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    void RpcAskMasterEnd(string reason, int actor)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        NetLog.Report("RoundEnd(Requested)", $"by:{actor} reason:{reason}");
        Net.Tools.SceneLoadGate.LoadLevelIfNeeded("Main");
    }
}

