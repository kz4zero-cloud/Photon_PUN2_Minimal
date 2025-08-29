// Assets/Scripts/Net/GameFlow/MainStagingReset.cs
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-10000)] // �ł��邾�������B���������s��InRoom��Ɍ���
public class MainStagingReset : MonoBehaviourPunCallbacks
{
    // PunStagingFlow �ƍ��킹���L�[
    const string RP_STATE = "gm_state"; // 0=Staging,1=Selected,2=ReadyCheck,3=Loading,4=InGame
    const string RP_MODE = "gm_mode";
    const string RP_SCENE = "gm_scene";

    public PunStagingFlow flow; // ���ݒ�Ȃ玩�����o
    bool didReset;

    void Awake()
    {
        if (!flow) flow = FindObjectOfType<PunStagingFlow>();
        TryReset("Awake");
        TrySyncUI("Awake");
    }

    void Start()
    {
        // Start���_�ł܂�Join����Ȃ��P�[�X�ɔ����čĎ��s��OnJoinedRoom���ɔC����
        TryReset("Start");
        TrySyncUI("Start");
    }

    public override void OnJoinedRoom()
    {
        TryReset("OnJoinedRoom");
        TrySyncUI("OnJoinedRoom");
    }

    void TryReset(string where)
    {
        if (didReset) return;
        if (!PhotonNetwork.InRoom)
        {
            NetLog.Report("MainReset", $"Defer reset ({where}) state:{PhotonNetwork.NetworkClientState}");
            return;
        }

        // �����̃t���O��p���X�V
        PunPropUtil.SetIfChanged(PhotonNetwork.LocalPlayer, "ready", false);
        PunPropUtil.SetIfChanged(PhotonNetwork.LocalPlayer, "spawned", false);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
            var rt = new Hashtable
            {
                [RP_MODE] = null,
                [RP_SCENE] = null,
                [RP_STATE] = 0, // Staging
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(rt);
        }

        didReset = true;
        NetLog.Report("MainReset", $"ResetNow ({where}) ready=false; Spawned=false; gm_state=Staging");
    }

    void TrySyncUI(string where)
    {
        // UI�̃g�O����InRoom�ゾ��
        if (!flow || !PhotonNetwork.InRoom)
        {
            NetLog.Report("MainReset", $"Defer UI sync ({where}) InRoom:{PhotonNetwork.InRoom}");
            return;
        }
        flow.SetReadyFromUI(false);
        NetLog.Report("MainReset", $"UI synced OFF ({where})");
    }

    // ������Spawned��true�ɖ߂������̕ی��i�K�v���̂ݍď������݁j
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changed)
    {
        if (target != PhotonNetwork.LocalPlayer || changed == null) return;
        if (changed.ContainsKey("spawned") && changed["spawned"] is bool b && b)
        {
            PunPropUtil.SetIfChanged(PhotonNetwork.LocalPlayer, "spawned", false);
            NetLog.Report("MainReset(Props)", "Forced Spawned=false");
        }
    }
}
