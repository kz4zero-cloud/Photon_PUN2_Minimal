using Photon.Pun;
using UnityEngine;

public class PunHud : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 600, 20), $"State: {PhotonNetwork.NetworkClientState}");
        GUI.Label(new Rect(10, 30, 600, 20), $"Connected:{PhotonNetwork.IsConnected}  InRoom:{PhotonNetwork.InRoom}  Room:{PhotonNetwork.CurrentRoom?.Name}");
        var prefabOk = Resources.Load<GameObject>("Prefabs/Player") != null;
        GUI.Label(new Rect(10, 50, 600, 20), $"Prefab( Prefabs/Player ): {(prefabOk ? "OK" : "MISSING")}");
    }
}
