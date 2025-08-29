using UnityEngine;
using Photon.Pun;

[DefaultExecutionOrder(-1000)]
public sealed class AutoSceneSyncOff : MonoBehaviour
{
    void Awake()
    {
        // �����O�ɕK��OFF�B�ȍ~�̃V�[���J�ڂ͑S�Ď��O�ōs���B
        PhotonNetwork.AutomaticallySyncScene = false;
        // �����L���[���O�̂���ON�ɁB
        PhotonNetwork.IsMessageQueueRunning = true;
    }
}
