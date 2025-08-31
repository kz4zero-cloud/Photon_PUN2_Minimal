using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Net.GameFlow; // GamePlayerSpawner

public class PunBootstrap : MonoBehaviourPunCallbacks
{
    private const string CP_Spawned = "spawned";

    private void Start()
    {
        // 🔴 ここを追加：シーン同期を有効化
        PhotonNetwork.AutomaticallySyncScene = true;
        NetLog.Report("BootstrapStart", SceneManager.GetActiveScene().name);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            ForceSpawnFlagFalse();      // ★ステージ開始時に必ず false に戻す
            CleanupMyLeftovers();       // 念のため残骸を掃除
            GamePlayerSpawner.RequestSpawn();   // スポーン要求（単一点）
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

        // 初回接続時にも同じ手順で安全にスポーン
        ForceSpawnFlagFalse();
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
        ClearSpawnedCP(); // 旧互換：離脱時に片付け
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        NetLog.Report("OnDisconnected", cause.ToString());
        ClearSpawnedCP();
    }

    // ====== 追加：毎シーン開始で "spawned=false" を強制 ======
    private void ForceSpawnFlagFalse()
    {
        try
        {
            var lp = PhotonNetwork.LocalPlayer;
            if (lp == null) return;

            var hash = lp.CustomProperties ?? new Hashtable();
            bool needSet =
                !hash.ContainsKey(CP_Spawned) ||
                (hash[CP_Spawned] is bool b && b); // true だったら false に戻す

            if (needSet)
            {
                hash[CP_Spawned] = false;
                lp.SetCustomProperties(hash);
                NetLog.Report("ForceSpawnFlag", "set=false");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PunBootstrap] ForceSpawnFlagFalse failed: " + e.Message);
        }
    }

    // ===== 残骸掃除（自分の所有オブジェクトのみ破棄）=====
    private void CleanupMyLeftovers()
    {
        var mine = FindObjectsOfType<PhotonView>()
            .Where(pv => pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
            .Select(pv => pv.gameObject)
            .Distinct()
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
        var lp = PhotonNetwork.LocalPlayer;
        if (lp == null) return;

        var hash = lp.CustomProperties;
        if (hash != null && hash.ContainsKey(CP_Spawned))
        {
            hash.Remove(CP_Spawned);
            lp.SetCustomProperties(hash);
        }
    }
}
