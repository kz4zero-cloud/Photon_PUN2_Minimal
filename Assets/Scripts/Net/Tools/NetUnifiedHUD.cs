using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class NetUnifiedHUD : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] bool showHud = true;
    [SerializeField] int corner = 0; // 0:TL 1:TR 2:BL 3:BR
    [SerializeField] Vector2 size = new Vector2(520, 220);

    Rect win;
    Vector2 scroll;

    void Start()
    {
        var w = (int)size.x;
        var h = (int)size.y;
        win = new Rect(10, 10, w, h);
        ApplyCorner();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6)) showHud = !showHud;
        if (Input.GetKeyDown(KeyCode.F7)) { corner = (corner + 1) % 4; ApplyCorner(); }
    }

    void ApplyCorner()
    {
        var w = (int)size.x;
        var h = (int)size.y;
        const int pad = 10;
        switch (corner)
        {
            case 0: win.position = new Vector2(pad, pad); break;
            case 1: win.position = new Vector2(Screen.width - w - pad, pad); break;
            case 2: win.position = new Vector2(pad, Screen.height - h - pad); break;
            case 3: win.position = new Vector2(Screen.width - w - pad, Screen.height - h - pad); break;
        }
        win.size = size;
    }

    void OnGUI()
    {
        if (!showHud) return;
        win = GUI.Window(GetInstanceID(), win, DrawWin, "NET HUD  (Drag to move / F6 Show/Hide / F7 Corner)");
    }

    void DrawWin(int id)
    {
        var txt = BuildReport();
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.Label(txt);
        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    string BuildReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("===== NET HUD REPORT =====");
        sb.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
        sb.AppendLine($"Connected: {PhotonNetwork.IsConnected}  Ready: {PhotonNetwork.IsConnectedAndReady}  InLobby: {PhotonNetwork.InLobby}  InRoom: {PhotonNetwork.InRoom}  Master: {PhotonNetwork.IsMasterClient}");
        sb.AppendLine($"Nick: {PhotonNetwork.NickName ?? "(null)"}  Room: {PhotonNetwork.CurrentRoom?.Name ?? "(none)"}  Players: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}");

        // Bootstrap / PunPlayer 実体数
        var boots = FindObjectsOfType<PunBootstrap>(true);
        var players = FindObjectsOfType<PunPlayer>(true);
        var mine = players.Count(p => p && p.photonView && p.photonView.IsMine);
        sb.AppendLine($"PunBootstrap in scene: {boots.Length}");
        sb.AppendLine($"PunPlayer objects in scene: {players.Length}  (mine: {mine})");

        // Prefabと必須コンポーネント
        var prefab = Resources.Load<GameObject>("Prefabs/Player");
        sb.AppendLine($"Resources 'Prefabs/Player': {(prefab ? "FOUND" : "MISSING")}");
        if (prefab)
        {
            bool hasPV = prefab.GetComponent<PhotonView>() != null;
            bool hasPP = prefab.GetComponent<PunPlayer>() != null;
            sb.AppendLine($"Checks -> PV: {(hasPV ? "OK" : "MISSING")}  PunPlayer: {(hasPP ? "OK" : "MISSING")}");
        }

        // TagObject / 相手
        var tag = PhotonNetwork.LocalPlayer?.TagObject;
        var tagName = tag is GameObject go ? go.name : (tag?.ToString() ?? "null");
        sb.AppendLine($"LocalPlayer.TagObject: {tagName}");
        var others = PhotonNetwork.PlayerListOthers?.Select(p => p?.NickName ?? "(null)") ?? Enumerable.Empty<string>();
        sb.AppendLine($"Remote players: {(others.Any() ? string.Join(", ", others) : "(none)")}");
        sb.AppendLine("==========================");
        return sb.ToString();
    }
}
