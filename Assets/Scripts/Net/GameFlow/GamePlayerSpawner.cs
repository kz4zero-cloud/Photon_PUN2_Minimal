namespace Net.GameFlow
{
    using System.Linq;
    using UnityEngine;
    using Photon.Pun;

    public class GamePlayerSpawner : MonoBehaviour
    {
        [Tooltip("生成するプレイヤープレハブ（Resources/Prefabs/Player を推奨）")]
        public GameObject playerPrefab;

        private static GamePlayerSpawner instance;

        private void Awake()
        {
            instance = this;
        }

        public static void RequestSpawn()
        {
            if (instance == null)
            {
                Debug.LogWarning("[GamePlayerSpawner] No spawner instance in scene.");
                return;
            }

            if (instance.HasLocalPlayerInstance())
            {
                Debug.Log("[GamePlayerSpawner] Already spawned → skip RequestSpawn");
                return;
            }

            instance.TrySpawn();
        }

        private void TrySpawn()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[GamePlayerSpawner] PlayerPrefab is not assigned!");
                return;
            }

            if (HasLocalPlayerInstance())
            {
                Debug.Log("[GamePlayerSpawner] Skip: local player instance already exists.");
                return;
            }

            var spawnPos = Vector3.zero;
            var spawnRot = Quaternion.identity;

            var playerObj = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, spawnRot, 0);
            if (playerObj != null)
            {
                Debug.Log("[GamePlayerSpawner] SpawnedLocal: " + playerObj.name);
            }
            else
            {
                Debug.LogError("[GamePlayerSpawner] Spawn failed!");
            }
        }

        private bool HasLocalPlayerInstance()
        {
            return FindObjectsOfType<PhotonView>()
                .Any(pv => pv.IsMine && pv.gameObject.CompareTag("Player"));
        }
    }
}
