using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;

public class MasterControlPanel : MonoBehaviour
{
    [SerializeField] Button forceStartButton;
    [SerializeField] Slider desiredPlayersSlider; // min=2 max=8
    [SerializeField] Toggle autoFillBotsToggle;

    void Start()
    {
        if (forceStartButton) forceStartButton.onClick.AddListener(OnForceStart);
        if (desiredPlayersSlider) desiredPlayersSlider.onValueChanged.AddListener(OnDesiredChanged);
        if (autoFillBotsToggle) autoFillBotsToggle.onValueChanged.AddListener(OnAutoFillChanged);
    }

    void OnForceStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PunGameFlow.Instance?.ForceStart();
    }

    void OnDesiredChanged(float v)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var rp = new Hashtable { { GFKeys.DESIRED_PLAYERS, Mathf.RoundToInt(v) } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);
    }

    void OnAutoFillChanged(bool on)
    {
        // 今回はインスペクタ設定優先のためNOP。必要ならRoomProp化可能。
    }
}
