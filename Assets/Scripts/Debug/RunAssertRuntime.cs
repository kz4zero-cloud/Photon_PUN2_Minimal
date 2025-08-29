// Assets/Scripts/Debug/RunAssertRuntime.cs
using System;
using System.Collections;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RunAssertRuntime : MonoBehaviourPunCallbacks
{
    [Header("Keys")]
    public KeyCode finishKey = KeyCode.F9;

    private bool _markedSpawn;
    private int _lastMyPlayers;

    public override void OnEnable()
    {
        base.OnEnable();

        var scene = SceneManager.GetActiveScene().name;
        RunAssert.Begin(scene);

        // Ready（シーン有効化時に1回）
        RunAssert.Mark("ready");

        StartCoroutine(WatchLoop());
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private IEnumerator WatchLoop()
    {
        var w = new WaitForSeconds(0.3f);
        while (true)
        {
            // Spawn: LocalPlayer.CustomProperties["spawned"]
            var lp = PhotonNetwork.LocalPlayer;
            bool spawned = lp != null && lp.CustomProperties != null &&
                           lp.CustomProperties.TryGetValue("spawned", out var v) &&
                           ToBool(v);

            if (spawned && !_markedSpawn)
            {
                _markedSpawn = true;
                RunAssert.Mark("spawned");
                RunAssert.AssertPass("spawn_once");
            }

            // 重複スポーン検知
            var myPlayers = FindObjectsOfType<PhotonView>()
                .Where(pv => pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
                .Select(pv => pv.gameObject).Distinct().Count();

            if (myPlayers > 1 && myPlayers != _lastMyPlayers)
                RunAssert.AssertFail("duplicate_spawn", $"mine={myPlayers}");

            _lastMyPlayers = myPlayers;

            // Finish キー
            if (Input.GetKeyDown(finishKey))
            {
                RunAssert.Mark("finish");
                RunAssert.AssertPass("finish_marked");
            }
            yield return w;
        }
    }

    public override void OnLeftRoom()
    {
        var mine = FindObjectsOfType<PhotonView>()
            .Where(pv => pv && pv.IsMine && pv.gameObject.CompareTag("Player"))
            .ToArray();
        if (mine.Length == 0) RunAssert.AssertPass("no_leftover_on_leave");
        else RunAssert.AssertFail("leftover_on_leave", $"mine={mine.Length}");
    }

    private void OnSceneUnloaded(Scene s) => RunAssert.End(s.name);

    private static bool ToBool(object o) { try { return Convert.ToBoolean(o); } catch { return false; } }
}
