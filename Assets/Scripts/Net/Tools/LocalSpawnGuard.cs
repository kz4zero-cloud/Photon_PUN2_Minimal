// Assets/Scripts/Net/Tools/LocalSpawnGuard.cs
// ダミー化：単一点スポーンへ移行したため非稼働。警告も抑止。
#pragma warning disable 0414
using UnityEngine;
using Photon.Pun;

public sealed class LocalSpawnGuard : MonoBehaviourPunCallbacks
{
    [Header("Deprecated (no longer used)")]
    [SerializeField] string playerPrefabPath = "Prefabs/Player";

    void Awake()
    {
        if (Application.isPlaying)
        {
            Debug.Log("[LocalSpawnGuard] Deprecated and disabled. Use GamePlayerSpawner.");
            enabled = false; // Start/Updateは止める
        }
    }
}
#pragma warning restore 0414
