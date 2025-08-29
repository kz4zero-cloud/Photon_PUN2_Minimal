using System.Linq;                // Any() �p
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Net.Tools;                  // �� LocalSpawnGate ��g�����߂ɒǉ�

/// <summary>
/// Proto �~�j�Q�[���̃����^�C�������F���L�����̃X�|�[�������S��
/// </summary>
public class GameProtoRuntime : MonoBehaviourPunCallbacks
{
    [Header("Spawn settings")]
    [SerializeField] float radius = 5.5f;
    [SerializeField] float y = 0.5f;

    [Header("Resources path")]
    [SerializeField] string playerPrefabPath = "Prefabs/Player"; // Resources/Prefabs/Player.prefab

    void Start()
    {
        // ���[���O�Ȃ牽����Ȃ�
        if (!PhotonNetwork.InRoom)
        {
            NetLog.Report("GameSpawn Failed", "Not in room");
            return;
        }

        // �����̓�d�X�|�[����}�~�i���l�̉۔���ɂ͎g��Ȃ��j
        if (!LocalSpawnGate.CanSpawnMeOnce())
            return;

        TrySpawnMe();
    }

    void TrySpawnMe()
    {
        try
        {
            // �p�x���蓖�āiActorNumber�x�[�X�j
            int me = PhotonNetwork.LocalPlayer.ActorNumber;
            int total = Mathf.Max(PhotonNetwork.CurrentRoom.PlayerCount, 1);
            float ang = ((me - 1) / (float)total) * Mathf.PI * 2f;

            Vector3 pos = new Vector3(Mathf.Cos(ang) * radius, y, Mathf.Sin(ang) * radius);

            // ���Ɏ����[�J���� Player ���c���Ă�����O�̂��ߔj��
            var mine = FindObjectsOfType<PhotonView>().Where(v => v != null && v.IsMine).ToArray();
            if (mine.Any(v => v != null && v.gameObject.name.StartsWith("Player")))
            {
                // �d��������ΌÂ�����Еt����i���S���j
                foreach (var v in mine)
                {
                    if (v != null && v.gameObject.name.StartsWith("Player"))
                        Destroy(v.gameObject);
                }
            }

            // [DISABLED_BY_TOOL] PhotonNetwork.Instantiate was here:
// PhotonNetwork.Instantiate(playerPrefabPath, pos, Quaternion.identity);
            LocalSpawnGate.MarkSpawnedMe(true);

            NetLog.Report("SpawnedLocal",
                $"idx:{me} total:{total} nick:{PhotonNetwork.LocalPlayer.NickName}");
        }
        catch (System.Exception ex)
        {
            NetLog.Report("SpawnFailed", ex.GetType().Name + " :: " + ex.Message);
        }
    }

    // �V�[������o��Ƃ��� Spawned ��߂�
    void OnDestroy()
    {
        LocalSpawnGate.MarkSpawnedMe(false);
    }
}
