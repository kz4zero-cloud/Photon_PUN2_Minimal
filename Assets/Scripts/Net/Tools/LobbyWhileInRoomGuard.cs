using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

[DefaultExecutionOrder(-31999)]
public class LobbyWhileInRoomGuard : MonoBehaviour
{
    [SerializeField] bool autoFix = false;
    [SerializeField] string mainSceneName = "Main";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene prev, Scene next)
    {
        if (!PhotonNetwork.InRoom) return;
        if (next.name != "Lobby") return;

        NetLog.Report("GuardHit", $"Lobby while InRoom. autoFix:{autoFix} -> {mainSceneName}");
        if (autoFix)
        {
            // 自動同期がONなら PhotonNetwork.LoadLevel を推奨
            Net.Tools.SceneLoadGate.LoadLevelIfNeeded(mainSceneName);
        }
    }
}

