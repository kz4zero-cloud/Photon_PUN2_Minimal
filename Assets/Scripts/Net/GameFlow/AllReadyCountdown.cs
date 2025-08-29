﻿using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable; // Photon の Hashtable を明示

public class AllReadyCountdown : MonoBehaviourPunCallbacks
{
    [Header("Conditions")]
    [SerializeField] int minPlayers = 2;           // 必要最小人数（1人検証は 1）
    [SerializeField] string sceneKey = "gm_scene"; // ルームRP：遷移先シーン名
    [SerializeField] string readyKey = "ready";    // プレイヤーRP：Ready フラグ

    [Header("Countdown")]
    [SerializeField] int seconds = 3;              // カウントダウン秒

    Coroutine counting;

    void Awake() { /* 依存なし */ }

    // 親が public なので public override + base 呼び出しに統一
    public override void OnEnable()
    {
        base.OnEnable();
        Reevaluate("Enable");
    }

    public override void OnJoinedRoom() { Reevaluate("OnJoined"); }
    public override void OnPlayerEnteredRoom(Player newPlayer) { Reevaluate("Enter"); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { Reevaluate("Left"); }
    public override void OnRoomPropertiesUpdate(Hashtable props) { Reevaluate("RoomProps"); }
    public override void OnPlayerPropertiesUpdate(Player t, Hashtable c)
    {
        Reevaluate("PlayerProps");
    }

    void Reevaluate(string tag)
    {
        if (!PhotonNetwork.InRoom)
        {
            NetLog.Report("Countdown", $"Defer {tag} (not in room)");
            return;
        }

        var room = PhotonNetwork.CurrentRoom;

        // シーン指定チェック
        string sceneName = null;
        bool hasScene = room.CustomProperties != null
            && room.CustomProperties.ContainsKey(sceneKey)
            && (sceneName = room.CustomProperties[sceneKey] as string) != null
            && sceneName.Length > 0;

        // 人数チェック
        int pCnt = room.PlayerCount;
        bool enough = pCnt >= Mathf.Max(1, minPlayers);

        // Ready 全員チェック
        bool allReady = true;
        foreach (var p in room.Players.Values)
        {
            var cp = p.CustomProperties;
            if (!(cp != null && cp.ContainsKey(readyKey) && cp[readyKey] is bool b && b))
            { allReady = false; break; }
        }

        // 条件未達ならカウント解除
        if (!hasScene || !enough || !allReady)
        {
            if (counting != null)
            {
                StopCoroutine(counting);
                counting = null;
                NetLog.Report("CountdownAbort", $"tag:{tag} scene:{hasScene} players:{pCnt}/{minPlayers} allReady:{allReady}");
            }
            return;
        }

        // Master が未稼働なら開始
        if (PhotonNetwork.IsMasterClient && counting == null)
        {
            counting = StartCoroutine(CoCountAndStart(sceneName));
            NetLog.Report("CountdownStart", $"sec:{seconds} scene:{sceneName}");
        }
    }

    IEnumerator CoCountAndStart(string sceneName)
    {
        for (int t = seconds; t > 0; t--)
        {
            NetLog.Report("CountdownTick", t.ToString());
            yield return new WaitForSeconds(1f);

            if (!StillValid(sceneName))
            {
                NetLog.Report("CountdownAbort", "Condition changed during count");
                counting = null;
                yield break;
            }
        }

        if (StillValid(sceneName))
        {
            NetLog.Report("CountdownGo", sceneName);
            // ここで他クラスは呼ばず、Master が直接ロードする
            Net.Tools.SceneLoadGate.LoadLevelIfNeeded(sceneName);
        }
        counting = null;
    }

    bool StillValid(string sceneName)
    {
        var room = PhotonNetwork.CurrentRoom; if (room == null) return false;

        // シーン継続
        if (!(room.CustomProperties != null
           && room.CustomProperties.ContainsKey(sceneKey)
           && (room.CustomProperties[sceneKey] as string) == sceneName)) return false;

        // 人数維持
        if (room.PlayerCount < Mathf.Max(1, minPlayers)) return false;

        // Ready 維持
        foreach (var p in room.Players.Values)
        {
            var cp = p.CustomProperties;
            if (!(cp != null && cp.ContainsKey(readyKey) && cp[readyKey] is bool b && b)) return false;
        }
        return true;
    }
}

