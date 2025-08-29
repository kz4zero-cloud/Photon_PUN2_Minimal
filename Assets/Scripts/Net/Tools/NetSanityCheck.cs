using System.Linq;
using System.Text;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetSanityCheck : MonoBehaviour
{
    public enum HudCorner { TopLeft, TopRight, BottomLeft, BottomRight }

    [Header("Player prefab path under Resources/")]
    [SerializeField] string playerPrefabPath = "Prefabs/Player";

    [Header("HUD")]
    [SerializeField] bool showHud = true;
    [SerializeField] HudCorner anchor = HudCorner.TopRight; // デフォルトで NetDiag と被らない
    [SerializeField] Vector2 offset = new Vector2(14, 14);
    [SerializeField] float width = 430f;
    [SerializeField] float height = 138f;
    [SerializeField] float alpha = 0.85f;
    [SerializeField] KeyCode cycleCornerKey = KeyCode.F7;   // 位置切替
    [SerializeField] KeyCode printReportKey = KeyCode.F8;   // 詳細レポート出力

    string _lastReport;

    void Update()
    {
        if (Input.GetKeyDown(cycleCornerKey))
            anchor = (HudCorner)(((int)anchor + 1) % 4);
        if (Input.GetKeyDown(printReportKey))
        {
            _lastReport = BuildReport();
            Debug.Log(_lastReport);
        }
    }

    void OnGUI()
    {
        if (!showHud) return;

        var prev = GUI.color;
        GUI.color = new Color(1, 1, 1, alpha);
        GUI.Box(GetRect(anchor, offset, width, height), BuildMini());
        GUI.color = new Color(1, 1, 1, alpha * 0.9f);
        if (!string.IsNullOrEmpty(_lastReport))
            GUI.Box(GetRect(anchor, offset + new Vector2(0, height + 6), width + 160, 200),
                    _lastReport.Length > 700 ? _lastReport.Substring(0, 700) + "..." : _lastReport);
        GUI.color = prev;
    }

    Rect GetRect(HudCorner c, Vector2 ofs, float w, float h)
    {
        float x = ofs.x, y = ofs.y;
        if (c == HudCorner.TopRight) x = Screen.width - w - ofs.x;
        if (c == HudCorner.BottomLeft) y = Screen.height - h - ofs.y;
        if (c == HudCorner.BottomRight) { x = Screen.width - w - ofs.x; y = Screen.height - h - ofs.y; }
        return new Rect(x, y, w, h);
    }

    string BuildMini()
    {
        var sb = new StringBuilder();
        sb.AppendLine("NET SANITY");
        sb.Append("Scene: ").AppendLine(SceneManager.GetActiveScene().name);
        sb.Append("Connected: ").Append(PhotonNetwork.IsConnected)
          .Append("  Ready: ").Append(PhotonNetwork.IsConnectedAndReady).AppendLine();
        sb.Append("InLobby: ").Append(PhotonNetwork.InLobby)
          .Append("  InRoom: ").Append(PhotonNetwork.InRoom)
          .Append("  Master: ").Append(PhotonNetwork.IsMasterClient).AppendLine();

        int boots = FindObjectsOfType<PunBootstrap>(true).Length;
        int playersAll = FindObjectsOfType<PunPlayer>(true).Length;
        int playersMine = FindObjectsOfType<PunPlayer>(true).Count(p => p.GetComponent<PhotonView>()?.IsMine == true);

        sb.Append("PunBootstrap in scene: ").Append(boots).AppendLine();
        sb.Append("PunPlayer objects in scene: ").Append(playersAll)
          .Append("  (mine: ").Append(playersMine).Append(")").AppendLine();

        var prefab = Resources.Load<GameObject>(playerPrefabPath);
        bool hasPV = prefab && prefab.GetComponent<PhotonView>();
        bool hasPP = prefab && prefab.GetComponent<PunPlayer>();
        sb.Append("Prefab '").Append(playerPrefabPath).Append("': ")
          .Append(prefab ? "FOUND" : "NOT FOUND").Append("  ")
          .Append("PV: ").Append(hasPV ? "OK" : "NONE").Append("  ")
          .Append("PunPlayer: ").Append(hasPP ? "OK" : "NONE").AppendLine();

        return sb.ToString();
    }

    string BuildReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("==== NET SANITY REPORT ====");
        sb.Append("Scene: ").AppendLine(SceneManager.GetActiveScene().name);
        sb.Append("ClientState: ").AppendLine(PhotonNetwork.NetworkClientState.ToString());
        sb.Append("Connected: ").Append(PhotonNetwork.IsConnected)
          .Append("  InLobby: ").Append(PhotonNetwork.InLobby)
          .Append("  InRoom: ").Append(PhotonNetwork.InRoom)
          .Append("  Master: ").AppendLine(PhotonNetwork.IsMasterClient.ToString());

        var boots = FindObjectsOfType<PunBootstrap>(true);
        sb.Append("PunBootstrap count: ").Append(boots.Length).AppendLine();

        var prefab = Resources.Load<GameObject>(playerPrefabPath);
        bool hasPV = prefab && prefab.GetComponent<PhotonView>();
        bool hasPP = prefab && prefab.GetComponent<PunPlayer>();
        sb.Append("Player prefab: ").AppendLine(prefab ? "FOUND" : "NOT FOUND");
        sb.Append(" - PhotonView: ").AppendLine(hasPV ? "OK" : "NONE");
        sb.Append(" - PunPlayer : ").AppendLine(hasPP ? "OK" : "NONE");
        if (!prefab || !hasPV || !hasPP) return sb.ToString();

        var players = FindObjectsOfType<PunPlayer>(true);
        sb.Append("PunPlayer in scene: ").Append(players.Length).AppendLine();

        var mine = players.FirstOrDefault(p => p.GetComponent<PhotonView>()?.IsMine == true);
        sb.Append("Local player found: ").AppendLine(mine ? "YES" : "NO");
        if (mine)
        {
            var pv = mine.GetComponent<PhotonView>();
            sb.Append("PhotonView.IsMine: ").AppendLine(pv.IsMine.ToString());
            var obs = pv.ObservedComponents;
            sb.Append("Observed count: ").AppendLine((obs != null ? obs.Count : 0).ToString());
        }

        if (PhotonNetwork.LocalPlayer != null)
        {
            var to = PhotonNetwork.LocalPlayer.TagObject as GameObject;
            sb.Append("LocalPlayer.TagObject: ").AppendLine(to ? to.name : "null");
        }

        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
        {
            var remotes = PhotonNetwork.CurrentRoom.Players.Values
                            .Where(p => !p.IsLocal).Select(p => p.NickName).ToArray();
            sb.Append("Remote players: ").AppendLine(remotes.Length > 0 ? string.Join(", ", remotes) : "(none)");
        }

        sb.AppendLine("===========================");
        return sb.ToString();
    }
}
