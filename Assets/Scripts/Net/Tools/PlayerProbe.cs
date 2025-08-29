using Photon.Pun;
using UnityEngine;

/// <summary>
/// ��������̃v���C���[�̂Ɏ����ŕt���A���t���[���ɂ킽���Ԃ����O����B
/// - PunPlayer/PhotonView �̗L��
/// - IsMine / ViewID
/// - ��莞�Ԍ�� PunPlayer ��������Ȃ��ꍇ�͋����x��
/// </summary>
public class PlayerProbe : MonoBehaviour
{
    void OnEnable()
    {
        Invoke(nameof(DumpNow), 0.02f);
        Invoke(nameof(DumpNow), 0.20f);
        Invoke(nameof(FinalCheck), 0.60f);
    }

    void DumpNow()
    {
        var pv = GetComponent<PhotonView>();
        var pp = GetComponent<PunPlayer>();
        Debug.Log($"[PlayerProbe] name={name} active={gameObject.activeInHierarchy} " +
                  $"PV={(pv ? "OK" : "NONE")} IsMine={(pv ? pv.IsMine : false)} ViewID={(pv ? pv.ViewID : 0)} " +
                  $"PunPlayer={(pp ? "OK" : "NONE")}");
    }

    void FinalCheck()
    {
        var pv = GetComponent<PhotonView>();
        var pp = GetComponent<PunPlayer>();
        if (pp == null)
        {
            Debug.LogError("[PlayerProbe] PunPlayer component STILL missing on spawned object. " +
                           "Prefab����/�X�N���v�g���s/Guid�j���̉\���BPlayer.prefab ���m�F���Ă��������B");
        }
    }
}
