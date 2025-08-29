using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PunReturnToLobby : MonoBehaviourPunCallbacks
{
    // UI�̃{�^������Ăяo��
    public void ReturnToLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // ���[���ޏo
        }
        else
        {
            SceneManager.LoadScene("Lobby"); // �O�̂���
        }
    }

    // ���[���𔲂�����ɌĂ΂��
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    // �G���[���̃n���h�����O�i�C�Ӂj
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);
        SceneManager.LoadScene("Lobby");
    }
}
