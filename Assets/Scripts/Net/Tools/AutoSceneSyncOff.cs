using UnityEngine;
using Photon.Pun;

[DefaultExecutionOrder(-1000)]
public sealed class AutoSceneSyncOff : MonoBehaviour
{
    void Awake()
    {
        // 入室前に必ずOFF。以降のシーン遷移は全て自前で行う。
        PhotonNetwork.AutomaticallySyncScene = false;
        // 同期キューも念のためONに。
        PhotonNetwork.IsMessageQueueRunning = true;
    }
}
