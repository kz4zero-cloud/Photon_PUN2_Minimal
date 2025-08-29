using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameRoundController : MonoBehaviourPunCallbacks
{
    private const string RP_STATE = "gm_state"; // 待機所と同じキー（状態を戻す用）
    public enum StagingState { Staging = 0, Selected = 1, ReadyCheck = 2, Loading = 3, InGame = 4 }

    [Header("Round")]
    [SerializeField] float autoEndSeconds = 20f; // テスト用：自動終了でMainへ戻す
    private bool endGate = false;

    void Start()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.IsMasterClient)
        {
            // ゲーム開始を状態に反映（通知用）
            var r = PhotonNetwork.CurrentRoom;
            var hash = r.CustomProperties;
            hash[RP_STATE] = (int)StagingState.InGame;
            r.SetCustomProperties(hash);
            NetLog.Report("GameStart", "InGame");
        }

        if (autoEndSeconds > 0f)
            Invoke(nameof(EndGameToMain), autoEndSeconds);
    }

    public void EndGameToMain()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (endGate) return;
        endGate = true;

        var r = PhotonNetwork.CurrentRoom;
        // 待機所に戻る準備：部屋を再び開放
        r.IsOpen = true; r.IsVisible = true;

        // 状態をStagingへ戻す（通知）
        var hash = r.CustomProperties;
        hash[RP_STATE] = (int)StagingState.Staging;
        r.SetCustomProperties(hash);

        NetLog.Report("GameEnd", "ReturnToMain");
        Net.Tools.SceneLoadGate.LoadLevelIfNeeded("Main");
    }
}

