using UnityEngine;
using Photon.Pun;

public static class PunSolo
{
    public static bool IsSolo { get; private set; }
    public static int BotCount { get; private set; } = 3;

    public static void StartSolo(int botCount = 3)
    {
        BotCount = Mathf.Max(0, botCount);
        IsSolo = true;

        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.CreateRoom("SOLO_" + Random.Range(1000, 9999));
    }

    public static void Reset()
    {
        IsSolo = false;
        BotCount = 3;
    }
}
