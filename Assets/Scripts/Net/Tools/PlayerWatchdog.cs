using Photon.Pun;
using UnityEngine;

/// <summary>
/// ���L����/���L�����̃��C�t�T�C�N�����ڍ׃��O�ɏo���Ď��p�X�N���v�g�B
/// </summary>
public class PlayerWatch : MonoBehaviour
{
    private PhotonView _pv;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        Log("Awake");
    }

    private void OnEnable()
    {
        Log("OnEnable");
    }

    private void Start()
    {
        Log("Start");
    }

    private void OnDisable()
    {
        Log("OnDisable");
    }

    private void OnDestroy()
    {
        Log("OnDestroy");
        // ���ꎩ���̖{�̂������R�[�h���ǂ����ɂ���΂����ō��Ղ�������
        if (_pv && _pv.IsMine)
        {
            if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.TagObject as GameObject == gameObject)
            {
                PhotonNetwork.LocalPlayer.TagObject = null;
                Debug.Log("[PlayerWatch] Cleared TagObject because the local player object is being destroyed.");
            }
        }
    }

    private void Log(string hook)
    {
        string owner = _pv ? (_pv.IsMine ? "MINE" : $"Remote({(_pv.Owner != null ? _pv.Owner.NickName : "null")})") : "no PV";
        Debug.Log($"[PlayerWatch] {hook}  name={gameObject.name}  owner={owner}  active={gameObject.activeInHierarchy}");
    }
}
