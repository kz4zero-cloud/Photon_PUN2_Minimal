using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PunReturnToLobby : MonoBehaviourPunCallbacks
{
    // UIのボタンから呼び出す
    public void ReturnToLobby()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); // ルーム退出
        }
        else
        {
            SceneManager.LoadScene("Lobby"); // 念のため
        }
    }

    // ルームを抜けた後に呼ばれる
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    // エラー時のハンドリング（任意）
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);
        SceneManager.LoadScene("Lobby");
    }
}
