using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Net.GameFlow; // GamePlayerSpawner のため

public class PunBootstrap : MonoBehaviourPunCallbacks
{
    // 旧設計互換で使うキー名（本クラスでは読み書きしないが、外部依存がある場合のため保持）
    private const string CP_Spawned = "spawned";

    private void Start()
    {
        NetLog.Report("BootstrapStart", SceneManager.GetActiveScene().name);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            CleanupMyLeftovers();                 // 自分の残骸を掃除（安全）
            GamePlayerSpawner.RequestSpawn();     // ★スポーンは中央スポーナーに一任★
        }
        else
        {
            NetLog.Report("BootstrapStart", "Waiting for room (not in room yet).");
        }
    }

    public override void OnJoinedRoom()
    {
        NetLog.Report("OnJoinedRoom",
            $"Room:{PhotonNetwork.CurrentRoom?.Name}, Count:{PhotonNetwork.CurrentRoom?.PlayerCount}");

        // どのシーンでも、中央スポーナーが居れば委譲
        CleanupMyLeftovers();
        GamePlayerSpawner.RequestSpawn();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        NetLog.Report("OnPlayerEntered", $"{newPlayer.NickName}({newPlayer.ActorNumber})");
    }

    public override void OnLeftRoom()
    {
        NetLog.Report("OnLeftRoom");
        ClearSpawnedCP(); // 旧設計互換（必要なければ残っていても害はない）
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        NetLog.Report("OnDisconnected", cause.ToString());
        ClearSpawnedCP();
    }

    // ===== 残骸掃除（自分の所有オブジェクトのみ壊す）=====
    private void CleanupMyLeftovers()
    {
        var mine = FindObjectsOfType<PhotonView>()
            .Where(pv => pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
            .Select(pv => pv.gameObject)
            .ToArray();

        if (mine.Length == 0) return;

        foreach (var obj in mine)
        {
            bool done = false;
            try { PhotonNetwork.Destroy(obj); done = true; } catch { }
            if (!done && obj != null) { try { Object.Destroy(obj); } catch { } }
        }

        NetLog.Report("CleanupMyLeftovers", $"Destroyed:{mine.Length}");
    }

    // ===== 旧互換：CustomProperties の spawned をリセット =====
    private void ClearSpawnedCP()
    {
        if (PhotonNetwork.LocalPlayer == null) return;
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (hash != null && hash.ContainsKey(CP_Spawned))
        {
            hash.Remove(CP_Spawned);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
}
