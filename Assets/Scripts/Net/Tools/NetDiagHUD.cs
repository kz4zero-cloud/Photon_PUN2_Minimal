using UnityEngine;
using Photon.Pun;
using System.Linq;

public class NetDiagHUD : MonoBehaviour
{
    Rect r = new Rect(10, 10, 460, 210);
    const string PrefabPath = "Prefabs/Player";

    void OnGUI()
    {
        GUI.Box(r, "NET DIAG");
        GUILayout.BeginArea(new Rect(r.x + 8, r.y + 24, r.width - 16, r.height - 32));
        GUILayout.Label($"Connected: {PhotonNetwork.IsConnected}   Ready: {PhotonNetwork.IsConnectedAndReady}");
        GUILayout.Label($"InLobby: {PhotonNetwork.InLobby}   InRoom: {PhotonNetwork.InRoom}   Master: {PhotonNetwork.IsMasterClient}");
        GUILayout.Label($"Nick: {PhotonNetwork.NickName}");
        var room = PhotonNetwork.CurrentRoom;
        GUILayout.Label($"Room: {(room != null ? room.Name : "(none)")}   Players: {(room != null ? room.PlayerCount : 0)}");
        var players = FindObjectsOfType<PunPlayer>();
        GUILayout.Label($"PunPlayer objects in scene: {players.Length}");

        // 追加: Prefab と Bootstrap の健全性チェック
        bool prefabOk = Resources.Load<GameObject>(PrefabPath) != null;
        bool bootstrapOk = FindObjectsOfType<PunBootstrap>().Any(go => go.isActiveAndEnabled);
        GUILayout.Label($"Prefab '{PrefabPath}': {(prefabOk ? "FOUND" : "MISSING")}   PunBootstrap: {(bootstrapOk ? "ACTIVE" : "MISSING")}");

        GUILayout.EndArea();
    }
}
