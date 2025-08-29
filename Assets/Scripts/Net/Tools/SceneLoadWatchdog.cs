using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

[DefaultExecutionOrder(-32000)]
public class SceneLoadWatchdog : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        NetLog.Report("SceneWatchdog", $"Init in:{SceneManager.GetActiveScene().name} InRoom:{PhotonNetwork.InRoom}");
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnActiveSceneChanged(Scene prev, Scene next)
    {
        NetLog.Report("SceneChanged",
            $"from:{prev.name} -> to:{next.name} InRoom:{PhotonNetwork.InRoom} Master:{PhotonNetwork.IsMasterClient}");
        if (PhotonNetwork.InRoom && next.name == "Lobby")
        {
            NetLog.Report("WARN:LobbyWhileInRoom",
                "ActiveScene changed to Lobby while still InRoom (someone loaded Lobby).");
        }
    }

    void OnSceneLoaded(Scene scn, LoadSceneMode mode)
    {
        NetLog.Report("SceneLoaded", $"{scn.name} mode:{mode} InRoom:{PhotonNetwork.InRoom}");
        if (PhotonNetwork.InRoom && scn.name == "Lobby")
        {
            NetLog.Report("WARN:LobbyLoadedInRoom",
                "SceneLoaded: Lobby while InRoom. Check callers (Bootstrap/StateChanged/etc).");
        }
    }
}
