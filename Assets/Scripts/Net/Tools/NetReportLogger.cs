// Assets/Scripts/Net/Tools/NetReportLogger.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class NetReportLogger : MonoBehaviourPunCallbacks
{
    [Header("Write a per-session log file under persistentDataPath")]
    [SerializeField] bool writeToFile = true;

    string logPath;
    StreamWriter writer;

    void Awake()
    {
        if (writeToFile)
        {
            var name = $"netlog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            logPath = Path.Combine(Application.persistentDataPath, name);
            try
            {
                writer = new StreamWriter(logPath, append: false, Encoding.UTF8);
                writer.AutoFlush = true;
                writer.WriteLine($"=== NET LOG START === {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Unity {Application.unityVersion}  |  Platform {Application.platform}  |  Product {Application.productName}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[NetLog] failed to open file: {e.Message}");
                writer = null;
            }
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (writer != null)
        {
            writer.WriteLine("=== NET LOG END ===");
            writer.Dispose();
            writer = null;
        }
    }

    void Start() => Dump("Start");

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Dump($"SceneLoaded:{scene.name}");

    // ===== Photon callbacks we care about =====
    public override void OnConnectedToMaster() => Dump("OnConnectedToMaster");
    public override void OnJoinedLobby() => Dump("OnJoinedLobby");
    public override void OnLeftLobby() => Dump("OnLeftLobby");
    public override void OnCreatedRoom() => Dump("OnCreatedRoom");
    public override void OnJoinedRoom() => Dump("OnJoinedRoom");
    public override void OnLeftRoom() => Dump("OnLeftRoom");
    public override void OnPlayerEnteredRoom(Player newPlayer) => Dump($"OnPlayerEntered:{Safe(newPlayer)}");
    public override void OnPlayerLeftRoom(Player otherPlayer) => Dump($"OnPlayerLeft:{Safe(otherPlayer)}");
    public override void OnDisconnected(DisconnectCause cause) => Dump($"OnDisconnected:{cause}");

    string Safe(UnityEngine.Object o) => o ? o.name : "null";
    string Safe(Player p) => p == null ? "null" : $"{p.NickName}({p.ActorNumber})";

    public void Dump(string reason)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("==== NET LOG REPORT ====");
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
            sb.AppendLine($"Connected: {PhotonNetwork.IsConnected}   Ready: {PhotonNetwork.IsConnectedAndReady}   InLobby: {PhotonNetwork.InLobby}   InRoom: {PhotonNetwork.InRoom}   Master: {PhotonNetwork.IsMasterClient}");
            sb.AppendLine($"Nick: {PhotonNetwork.NickName ?? "(null)"}   Room: {PhotonNetwork.CurrentRoom?.Name ?? "(none)"}   Players: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}");

            // Bootstrap instances in the scene
            var boots = FindObjectsOfType<PunBootstrap>(includeInactive: true);
            sb.AppendLine($"PunBootstrap in scene: {boots.Length}");

            // Local/remote PunPlayer objects
            var punPlayers = FindObjectsOfType<PunPlayer>(includeInactive: true);
            var mineCount = punPlayers.Count(p => p != null && p.photonView != null && p.photonView.IsMine);
            sb.AppendLine($"PunPlayer objects in scene: {punPlayers.Length}  (mine: {mineCount})");

            // Prefab & components check
            var prefab = Resources.Load<GameObject>("Prefabs/Player");
            sb.AppendLine($"Resources.Load('Prefabs/Player'): {(prefab ? "FOUND" : "MISSING")}");
            if (prefab)
            {
                bool hasPV = prefab.GetComponent<PhotonView>() != null;
                bool hasComp = prefab.GetComponent<PunPlayer>() != null;
                sb.AppendLine($"Prefab comps: {string.Join(", ", prefab.GetComponents<Component>().Select(c => c?.GetType().Name).Where(n => !string.IsNullOrEmpty(n)))}");
                sb.AppendLine($"Checks -> PV: {(hasPV ? "OK" : "MISSING")}   PunPlayer: {(hasComp ? "OK" : "MISSING")}");
            }

            // TagObject & remotes
            var tagObjName = PhotonNetwork.LocalPlayer?.TagObject is GameObject go ? go.name : (PhotonNetwork.LocalPlayer?.TagObject?.ToString() ?? "null");
            sb.AppendLine($"LocalPlayer.TagObject: {tagObjName}");
            var others = PhotonNetwork.PlayerListOthers?.Select(p => p?.NickName ?? "(null)")?.ToArray() ?? Array.Empty<string>();
            sb.AppendLine($"Remote players: {(others.Length == 0 ? "(none)" : string.Join(", ", others))}");
            sb.AppendLine("========================");

            var text = sb.ToString();
            Debug.Log(text);
            if (writer != null) writer.WriteLine(text);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[NetLog] Dump failed: {e.Message}");
            if (writer != null) writer.WriteLine($"[NetLog] Dump failed: {e}");
        }
    }
}
