using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreService : MonoBehaviourPunCallbacks, IOnEventCallback
{
    Dictionary<int, int> pending = new Dictionary<int, int>(); // actorNumber -> delta
    float flushTimer;
    [SerializeField] float flushInterval = 0.33f; // 0.2〜0.5s 推奨

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.OnDisable();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != GFKeys.EV_SCORE_DELTA) return;

        // Master（またはOffline）でのみ確定
        if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) return;

        var data = photonEvent.CustomData as object[];
        if (data == null || data.Length < 2) return;
        int actor = (int)data[0];
        int delta = (int)data[1];

        if (!pending.ContainsKey(actor)) pending[actor] = 0;
        pending[actor] += delta;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) return;

        flushTimer += Time.deltaTime;
        if (flushTimer < flushInterval) return;
        flushTimer = 0f;

        foreach (var kv in pending)
        {
            var player = FindPlayer(kv.Key);
            if (player == null) continue;

            int cur = 0;
            if (player.CustomProperties.TryGetValue(GFKeys.SCORE, out var v) && v is int s) cur = s;
            var props = new Hashtable { { GFKeys.SCORE, cur + kv.Value } };
            player.SetCustomProperties(props);
        }
        pending.Clear();
    }

    Player FindPlayer(int actorNumber)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.ActorNumber == actorNumber) return p;
        return null;
    }
}
