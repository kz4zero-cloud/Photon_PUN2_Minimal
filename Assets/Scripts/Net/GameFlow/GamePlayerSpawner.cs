// Assets/Scripts/Net/GameFlow/GamePlayerSpawner.cs
// �v���C���[�����̒P��_�B�����u�ȊO�v�� PhotonNetwork.Instantiate ���ĂԂ̂͋֎~�B
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Net.GameFlow
{
    public sealed class GamePlayerSpawner : MonoBehaviourPunCallbacks
    {
        public static GamePlayerSpawner Instance { get; private set; }

        [Header("Player Prefab (Resources ��)")]
        [Tooltip("��: Assets/Resources/Prefabs/Player.prefab �����蓖��")]
        public GameObject playerPrefab;

        [Header("Spawn Points (�C��)")]
        public Transform[] spawnPoints;

        [Header("Debug")]
        [Tooltip("�J���p: �V�[���J�n���Ɏ����X�|�[���i�{�Ԃł�OFF�j")]
        public bool spawnOnStartDebug = true;

        private bool localSpawned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GamePlayerSpawner] Duplicate in scene. Destroying this component.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            localSpawned = GetPlayerPropBool(PhotonNetwork.LocalPlayer, "spawned");
            if (spawnOnStartDebug) TrySpawnLocal();
        }

        public static void RequestSpawn() => Instance?.TrySpawnLocal();

        private void TrySpawnLocal()
        {
            if (localSpawned || GetPlayerPropBool(PhotonNetwork.LocalPlayer, "spawned"))
            {
                Debug.Log("[GamePlayerSpawner] Skip: already spawned."); return;
            }
            if (playerPrefab == null) { Debug.LogError("[GamePlayerSpawner] Player Prefab is not set."); return; }
            if (!PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.OfflineMode)
            { Debug.LogWarning("[GamePlayerSpawner] Photon is not ready."); return; }

            GetSpawnTransform(out var pos, out var rot);

            // �� �B��� Instantiate �Ăяo���_ ��
            var go = PhotonNetwork.Instantiate(playerPrefab.name, pos, rot, 0);
            if (go == null) { Debug.LogError("[GamePlayerSpawner] Instantiate failed."); return; }

            localSpawned = true;
            SetPlayerPropBool(PhotonNetwork.LocalPlayer, "spawned", true);
            Debug.Log("[GamePlayerSpawner] Spawned player once.");
        }

        private void GetSpawnTransform(out Vector3 pos, out Quaternion rot)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int idx = 0;
                if (PhotonNetwork.LocalPlayer != null)
                    idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1 + spawnPoints.Length) % spawnPoints.Length;
                var t = spawnPoints[idx % spawnPoints.Length];
                pos = t.position; rot = t.rotation;
            }
            else { pos = Vector3.zero; rot = Quaternion.identity; }
        }

        private static bool GetPlayerPropBool(Player p, string key)
        {
            if (p == null || p.CustomProperties == null) return false;
            if (p.CustomProperties.TryGetValue(key, out var v))
            { try { return Convert.ToBoolean(v); } catch { return false; } }
            return false;
        }
        private static void SetPlayerPropBool(Player p, string key, bool val)
        {
            if (p == null) return;
            var table = new ExitGames.Client.Photon.Hashtable { { key, val } };
            p.SetCustomProperties(table);
        }
    }
}
