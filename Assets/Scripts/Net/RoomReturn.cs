// •Û‘¶æ: Assets/Scripts/Net/RoomReturn.cs
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RoomReturn : MonoBehaviourPunCallbacks
{
    [SerializeField] private string lobbySceneName = "Lobby";

    // 0: –¢—£’E / 1: —£’Eˆ—’†i‘½d‰Ÿ‰º‚â‹£‡–hŽ~j
    private static int _leaving = 0;

    // ƒ{ƒ^ƒ“‚©‚çŒÄ‚Ô
    public void BackToLobby()
    {
        if (Interlocked.Exchange(ref _leaving, 1) == 1)
        {
            NetLog.Report("BackToLobby", "Ignored: already leaving");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            NetLog.Report("LeaveRoomRequested", "via BackToLobby");
            try { PhotonNetwork.LeaveRoom(); }
            catch (System.Exception ex)
            {
                NetLog.Report("LeaveRoomException", ex.GetType().Name + ": " + ex.Message);
                ResetGate();
            }
        }
        else
        {
            NetLog.Report("LoadLobbyDirect", "Not in room");
            LoadLobby();
        }
    }

    public override void OnLeftRoom()
    {
        NetLog.Report("OnLeftRoom", "Load Lobby");
        LoadLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        NetLog.Report("OnDisconnected", "cause:" + cause + " -> Load Lobby");
        LoadLobby();
    }

    private void LoadLobby()
    {
        try
        {
            SceneManager.LoadScene(lobbySceneName);
            NetLog.Report("LoadScene", lobbySceneName);
        }
        finally
        {
            ResetGate();
        }
    }

    private void ResetGate() => Interlocked.Exchange(ref _leaving, 0);
}
