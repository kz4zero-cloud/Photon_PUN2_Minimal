using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class BotManagerOnline : MonoBehaviour
{
    [SerializeField] string botRoomPrefabName = "Bot_Networked"; // Resources/ íºâ∫
    readonly List<GameObject> bots = new List<GameObject>();

    public int CurrentBotCount => bots.Count;

    public void EnsureBotCount(int target)
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode) return;
        target = Mathf.Max(0, target);

        // ëùÇ‚Ç∑
        while (bots.Count < target)
        {
            var go = PhotonNetwork.InstantiateRoomObject(botRoomPrefabName, RandomSpawn(), Quaternion.identity);
            bots.Add(go);
        }
        // å∏ÇÁÇ∑
        while (bots.Count > target)
        {
            var last = bots[bots.Count - 1];
            bots.RemoveAt(bots.Count - 1);
            if (last && last.GetComponent<PhotonView>()) PhotonNetwork.Destroy(last);
        }
    }

    Vector3 RandomSpawn() => new Vector3(Random.Range(-3f, 3f), 1.0f, Random.Range(-3f, 3f));
}
