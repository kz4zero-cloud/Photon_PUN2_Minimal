using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class GameProtoReturnUI : MonoBehaviourPun
{
    bool pressed;

    public void ReturnToWaiting()
    {
        if (pressed) return;
        pressed = true;

        if (PhotonNetwork.IsMasterClient)
        {
            NetLog.Report("GameEnd(Button)", "Master -> Load Main");
            Net.Tools.SceneLoadGate.LoadLevelIfNeeded("Main");
        }
        else
        {
            NetLog.Report("GameEnd(Button)", "Request to Master");
            photonView.RPC(nameof(RpcAskMasterReturn), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    void RpcAskMasterReturn(int fromActor)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        NetLog.Report("GameEnd(Requested)", $"by actor:{fromActor}");
        Net.Tools.SceneLoadGate.LoadLevelIfNeeded("Main");
    }
}

