using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PunStagingFlow : MonoBehaviourPunCallbacks
{
    // RoomProps
    private const string RP_STATE = "gm_state"; // 0=Staging,1=Selected,2=ReadyCheck,3=Loading,4=InGame
    private const string RP_MODE = "gm_mode";  // e.g. "Proto"
    private const string RP_SCENE = "gm_scene"; // e.g. "Game_Proto"
    // PlayerProps
    private const string PP_READY = "ready";

    public enum StagingState { Staging = 0, Selected = 1, ReadyCheck = 2, Loading = 3, InGame = 4 }

    [Header("Settings")]
    [SerializeField] int minPlayers = 2;

    private bool loadGate = false; // 多重ロード防止

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.InRoom) return;

        // 入室時は必ず自分の ready を false に初期化
        SetMyReady(false, "EnterMain");

        if (PhotonNetwork.IsMasterClient)
        {
            var r = PhotonNetwork.CurrentRoom;
            if (!r.CustomProperties.ContainsKey(RP_STATE))
            {
                SetState(StagingState.Staging);
                NetLog.Report("StagingInit", "Set Staging");
            }
            DumpStatus("Start"); // ←現状の一覧
            var s = GetState(r);
            if (s == StagingState.Selected || s == StagingState.ReadyCheck)
                TryProceedIfAllReady("Resume");
        }
    }

    void Update()
    {
        // F8 でいつでもスナップショット
        if (Input.GetKeyDown(KeyCode.F8))
            DumpStatus("F8");
    }

    // ===== UI hooks =====
    public void SelectMode_Proto()
    {
        if (!PhotonNetwork.IsMasterClient) { NetLog.Report("ModeSelectIgnored", "NotMaster"); return; }

        string mode = "Proto";
        string scene = "Game_Proto";

        var r = PhotonNetwork.CurrentRoom;
        var hash = r.CustomProperties;
        hash[RP_MODE] = mode;
        hash[RP_SCENE] = scene;
        hash[RP_STATE] = (int)StagingState.Selected;
        r.SetCustomProperties(hash);

        NetLog.Report("ModeSelected", $"mode:{mode} scene:{scene}");
        loadGate = false;
        DumpStatus("AfterModeSelect");
        TryProceedIfAllReady("ModeSelected");
    }

    public void SetReadyFromUI(bool on)
    {
        SetMyReady(on, "UI");
        DumpStatus("AfterReadyToggle");
        if (PhotonNetwork.IsMasterClient) TryProceedIfAllReady("ReadyToggle");
    }

    // ===== internals =====
    private void SetMyReady(bool on, string from)
    {
        var lp = PhotonNetwork.LocalPlayer;
        if (lp == null) return;

        var hash = lp.CustomProperties;
        hash[PP_READY] = on;
        lp.SetCustomProperties(hash);
        NetLog.Report("ReadyChanged", $"actor:{lp.ActorNumber} -> {on} ({from})");
    }

    private void TryProceedIfAllReady(string from)
    {
        var r = PhotonNetwork.CurrentRoom; if (r == null) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (loadGate) { NetLog.Report("LoadGateClosed", from); return; }

        var s = GetState(r);
        if (s != StagingState.Selected && s != StagingState.ReadyCheck) return;

        NetLog.Report("StageCheck", $"from:{from} state:{s}");

        // シーン決定チェック
        if (!r.CustomProperties.TryGetValue(RP_SCENE, out var sceneObj) || string.IsNullOrEmpty(sceneObj as string))
        {
            NetLog.Report("ProceedGuard", "SceneNotSet");
            DumpStatus("SceneNotSet");
            return;
        }
        string sceneName = (string)sceneObj;

        // 人数チェック
        if (r.PlayerCount < minPlayers)
        {
            NetLog.Report("ProceedGuard", "NotEnoughPlayers");
            DumpStatus("NotEnoughPlayers");
            return;
        }

        // Ready 全員チェック
        if (!AreAllReady(out var missing))
        {
            NetLog.Report("ProceedGuard", $"NotAllReady missing:{string.Join(",", missing)}");
            return;
        }

        // ここまで来たら進む
        SetState(StagingState.ReadyCheck);
        loadGate = true;

        r.IsOpen = false; r.IsVisible = false;
        NetLog.Report("RoomClosed", $"players:{r.PlayerCount}");

        SetState(StagingState.Loading);
        NetLog.Report("LoadGame", sceneName);
        Net.Tools.SceneLoadGate.LoadLevelIfNeeded(sceneName);
    }

    private bool AreAllReady(out List<int> missingActors)
    {
        missingActors = new List<int>();
        foreach (var p in PhotonNetwork.PlayerList)
        {
            bool ready = p.CustomProperties.TryGetValue(PP_READY, out var v) && v is bool b && b;
            if (!ready) missingActors.Add(p.ActorNumber);
        }
        if (missingActors.Count == 0)
        {
            NetLog.Report("AllReady", $"count:{PhotonNetwork.PlayerList.Length}");
            return true;
        }
        return false;
    }

    private StagingState GetState(Room r)
    {
        if (r.CustomProperties.TryGetValue(RP_STATE, out var v))
            return (StagingState)(int)(v ?? 0);
        return StagingState.Staging;
    }

    private void SetState(StagingState s)
    {
        var r = PhotonNetwork.CurrentRoom; if (r == null) return;
        var hash = r.CustomProperties;
        hash[RP_STATE] = (int)s;
        r.SetCustomProperties(hash);
        NetLog.Report("StateChanged", s.ToString());
    }

    // ===== callbacks =====
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PP_READY))
        {
            NetLog.Report("ReadyPropChanged", $"actor:{target.ActorNumber} -> {changedProps[PP_READY]}");
            DumpStatus("PropsUpdate");
            if (PhotonNetwork.IsMasterClient) TryProceedIfAllReady("PropsUpdate");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        NetLog.Report("OnPlayerEntered", $"{newPlayer.NickName}({newPlayer.ActorNumber})");
        DumpStatus("Enter");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        NetLog.Report("OnPlayerLeft", $"{otherPlayer.NickName}({otherPlayer.ActorNumber})");
        DumpStatus("Left");

        if (!PhotonNetwork.IsMasterClient) return;

        var r = PhotonNetwork.CurrentRoom;
        var s = GetState(r);
        if ((s == StagingState.Selected || s == StagingState.ReadyCheck) && r.PlayerCount < minPlayers)
        {
            r.IsOpen = true; r.IsVisible = true;
            SetState(StagingState.Staging);
            loadGate = false;
            NetLog.Report("Rollback", "PlayerLeft");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        NetLog.Report("MasterSwitched", $"{newMasterClient.NickName}({newMasterClient.ActorNumber})");
        DumpStatus("MasterSwitched");
        if (PhotonNetwork.IsMasterClient)
        {
            loadGate = false;
            TryProceedIfAllReady("MasterSwitched");
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(RP_STATE))
        {
            if (changed[RP_STATE] is int s)
                NetLog.Report("StateChanged(Notify)", ((StagingState)s).ToString());
        }
    }

    // ===== diagnostics =====
    private void DumpStatus(string tag)
    {
        var r = PhotonNetwork.CurrentRoom;
        string sceneName = (r != null && r.CustomProperties.TryGetValue(RP_SCENE, out var so) && so is string sc) ? sc : "(none)";
        var state = r != null ? GetState(r) : StagingState.Staging;
        NetLog.Report("ReadySnapshot", $"{tag} state:{state} players:{PhotonNetwork.PlayerList.Length}/{minPlayers} scene:{sceneName}");
        foreach (var p in PhotonNetwork.PlayerList)
        {
            bool ready = p.CustomProperties.TryGetValue(PP_READY, out var v) && v is bool b && b;
            bool isLocal = (p == PhotonNetwork.LocalPlayer);
            NetLog.Report("ReadyEntry", $"actor:{p.ActorNumber} nick:{p.NickName} ready:{ready} local:{isLocal} master:{p.IsMasterClient}");
        }
    }
}

