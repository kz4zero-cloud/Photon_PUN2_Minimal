// Assets/Scripts/Net/Tools/LocalSpawnGuarantee.cs
// ダミー化：単一点スポーンへ移行したため非稼働。警告は無効化。
#pragma warning disable 0414
using UnityEngine;
using Photon.Pun;

public class LocalSpawnGuarantee : MonoBehaviourPunCallbacks
{
    [Header("Deprecated (no longer used)")]
    [SerializeField] string sceneName = "Main";
    [SerializeField] string prefabId = "Prefabs/Player";
    [SerializeField] float y = 0.5f;
    [SerializeField] float radius = 5.5f;

    void Awake()
    {
        if (Application.isPlaying)
        {
            Debug.Log("[LocalSpawnGuarantee] Deprecated and disabled. Use GamePlayerSpawner.");
            enabled = false; // Start/Updateは走らない
        }
    }
}
#pragma warning restore 0414
