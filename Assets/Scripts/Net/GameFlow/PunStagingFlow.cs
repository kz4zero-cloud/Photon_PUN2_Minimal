// Assets/Scripts/Net/GameFlow/PunStagingFlow.cs
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Newtonsoft.Json;

public class PunStagingFlow : MonoBehaviourPunCallbacks
{
    private const string RP_PLAN = "gm_plan";   // ステージプラン(JSON配列)
    private const string RP_INDEX = "gm_index";  // 現在のステージ番号
    private const string RP_READY = "ready";     // Readyフラグ

    private string lastLoadedScene = "";

    private void Awake()
    {
        Debug.Log("[Diag][PunStagingFlow] Awake scene=" +
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        Debug.Log("[Diag][PunStagingFlow] OnEnable (isActive=" + this.isActiveAndEnabled + ")");
    }

    private void Start()
    {
        Debug.Log("[Diag][PunStagingFlow] Start inRoom=" + PhotonNetwork.InRoom);
    }

    // ==== UIから呼ぶ ====

    public void SetReadyFromUI(bool isReady)
    {
        var hash = new Hashtable { { RP_READY, isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("[Diag][PunStagingFlow] SetReadyFromUI -> " + isReady);
    }

    public void SetPlanOnly(List<string> stages)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Diag][PunStagingFlow] SetPlanOnly called on non-Master");
            return;
        }

        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning("[Diag][PunStagingFlow] SetPlanOnly called with empty list");
            return;
        }

        string json = JsonConvert.SerializeObject(stages);
        var hash = new Hashtable
        {
            { RP_PLAN, json },
            { RP_INDEX, 0 }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        Debug.Log("[Diag][PunStagingFlow] SetPlanOnly saved plan=" + json);
    }

    // ==== Photon コールバック ====

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("[Diag][PunStagingFlow] OnRoomPropertiesUpdate keys=" +
            string.Join(",", propertiesThatChanged.Keys));

        if (PhotonNetwork.CurrentRoom != null)
        {
            foreach (var kv in PhotonNetwork.CurrentRoom.CustomProperties)
            {
                Debug.Log("[Diag][RoomProp] " + kv.Key + "=" + kv.Value);
            }
        }

        // シーンロード試行
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RP_PLAN) &&
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RP_INDEX))
        {
            string json = PhotonNetwork.CurrentRoom.CustomProperties[RP_PLAN] as string;
            int index = (int)PhotonNetwork.CurrentRoom.CustomProperties[RP_INDEX];

            List<string> stages = JsonConvert.DeserializeObject<List<string>>(json);
            if (index >= 0 && index < stages.Count)
            {
                string sceneName = stages[index];
                if (sceneName != lastLoadedScene)
                {
                    lastLoadedScene = sceneName;
                    Debug.Log("[Diag][PunStagingFlow] LoadLevel -> " + sceneName);
                    PhotonNetwork.LoadLevel(sceneName);
                }
            }
            else
            {
                Debug.LogWarning("[Diag][PunStagingFlow] Stage index out of range");
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("[Diag][PunStagingFlow] OnPlayerPropertiesUpdate player=" +
            targetPlayer.NickName + " props=" + changedProps.ToStringFull());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Diag][PunStagingFlow] OnJoinedRoom room=" +
            PhotonNetwork.CurrentRoom.Name + " players=" + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[Diag][PunStagingFlow] OnPlayerEnteredRoom -> " + newPlayer.NickName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("[Diag][PunStagingFlow] OnPlayerLeftRoom -> " + otherPlayer.NickName);
    }
}
