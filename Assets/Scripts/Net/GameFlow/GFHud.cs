using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class GFHud : MonoBehaviour
{
    Rect r = new Rect(12, 70, 320, 110);

    void OnGUI()
    {
        if (!PhotonNetwork.InRoom) return;

        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        GFState state = GFState.Waiting;
        double t0 = 0; int round = 0; int mini = 0;

        if (room.CustomProperties.TryGetValue(GFKeys.STATE, out var s)) state = (GFState)(byte)(s);
        if (room.CustomProperties.TryGetValue(GFKeys.T0, out var t)) t0 = (double)t;
        if (room.CustomProperties.TryGetValue(GFKeys.ROUND, out var rN)) round = (int)rN;
        if (room.CustomProperties.TryGetValue(GFKeys.MINI, out var mN)) mini = (int)mN;

        double now = PhotonNetwork.Time;
        int secs = Mathf.Max(0, Mathf.CeilToInt((float)(t0 - now)));

        GUI.Box(r, "ROUND HUD");
        GUILayout.BeginArea(new Rect(r.x + 8, r.y + 22, r.width - 16, r.height - 30));
        GUILayout.Label($"State : {state}");
        GUILayout.Label($"Round : {round + 1}   Mini : {mini}");
        if (state == GFState.Countdown) GUILayout.Label($"Starts in : {secs}s");
        if (state == GFState.Playing) GUILayout.Label($"Time left : {secs}s");
        GUILayout.EndArea();
    }
}
