using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunGameFlow : MonoBehaviourPunCallbacks
{
    public static PunGameFlow Instance { get; private set; }

    [Header("Config")]
    [SerializeField] MinigameRegistry registry;
    [SerializeField] int totalRounds = 3;

    [Header("Online Options")]
    [SerializeField] StartPolicy startPolicy = StartPolicy.HostForce;
    [SerializeField] int desiredPlayers = 6; // humans+bots
    [SerializeField] bool autoFillBotsOnline = true;
    [SerializeField] int botsMax = 6;
    [SerializeField] int minHumansToContinue = 2;

    [Header("HUD (optional)")]
    [SerializeField] TMP_Text statusText;

    [Header("Refs")]
    [SerializeField] BotManagerOnline botManager;
    [SerializeField] ScoreService scoreService;

    int peakHumanCountThisRound = 1;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (!botManager) botManager = GetComponent<BotManagerOnline>();
        if (!scoreService) scoreService = GetComponent<ScoreService>();
    }

    void Start()
    {
        if (!PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            var rp = new Hashtable();
            rp[GFKeys.SES_STATE] = (byte)GameState.Waiting;
            rp[GFKeys.ROUND_IDX] = 0;
            rp[GFKeys.START_POLICY] = (byte)startPolicy;
            rp[GFKeys.DESIRED_PLAYERS] = desiredPlayers;
            PhotonNetwork.CurrentRoom.SetCustomProperties(rp);
        }
        UpdateHUD();
    }

    // Masterの「強制開始」
    public void ForceStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int minDesired = Mathf.Max(2, desiredPlayers);
        if (startPolicy != StartPolicy.HostForce || minDesired < 2) return;

        var rp = new Hashtable();
        rp[GFKeys.SES_STATE] = (byte)GameState.RoundReady;
        rp[GFKeys.ROUND_IDX] = 0;
        rp[GFKeys.MG_KEY] = registry && registry.order.Length > 0 ? registry.order[0].key : "default";
        rp[GFKeys.SEED] = Random.Range(0, int.MaxValue);
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);

        BeginCountdownForRound(0);
    }

    void BeginCountdownForRound(int roundIdx)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var def = GetDef(roundIdx);
        if (def == null) { Debug.LogWarning("MinigameDef missing."); return; }

        // Bot補充（Countdown）
        if (autoFillBotsOnline && !PhotonNetwork.OfflineMode && botManager)
        {
            int humans = PhotonNetwork.PlayerList.Length;
            int target = Mathf.Clamp(desiredPlayers - humans, 0, botsMax);
            botManager.EnsureBotCount(target);
        }

        double end = PhotonNetwork.Time + def.countdownSec;
        var rp = new Hashtable { { GFKeys.SES_STATE, (byte)GameState.Countdown }, { GFKeys.CD_END, end } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);

        peakHumanCountThisRound = Mathf.Max(peakHumanCountThisRound, CountHumans());
    }

    void BeginRound(int roundIdx)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var def = GetDef(roundIdx);
        if (def == null) return;

        double end = PhotonNetwork.Time + def.timeLimitSec;
        var rp = new Hashtable { { GFKeys.SES_STATE, (byte)GameState.InRound }, { GFKeys.ROUND_END, end } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);

        peakHumanCountThisRound = Mathf.Max(CountHumans(), peakHumanCountThisRound);
        ApplyRoundLocally(def);
    }

    void FinishRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var rp = new Hashtable { { GFKeys.SES_STATE, (byte)GameState.Intermission } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);

        // IntermissionでBot再調整
        if (autoFillBotsOnline && !PhotonNetwork.OfflineMode && botManager)
        {
            int humans = CountHumans();
            int targetBots = Mathf.Clamp(desiredPlayers - humans, 0, botsMax);
            botManager.EnsureBotCount(targetBots);
        }
    }

    void Update()
    {
        UpdateHUD();

        if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) return;

        var state = GetState();
        switch (state)
        {
            case GameState.Countdown:
                if (Remain(GFKeys.CD_END) <= 0) BeginRound(GetRoundIndex());
                if (CountHumans() == 0) SetState(GameState.Waiting);
                break;

            case GameState.InRound:
                int humans = CountHumans();
                peakHumanCountThisRound = Mathf.Max(peakHumanCountThisRound, humans);
                if (peakHumanCountThisRound >= 2 && humans <= Mathf.Max(1, minHumansToContinue - 1))
                    SetState(GameState.Intermission);
                else if (Remain(GFKeys.ROUND_END) <= 0)
                    SetState(GameState.Intermission);
                break;

            case GameState.Intermission:
                int idx = GetRoundIndex() + 1;
                if (idx >= totalRounds) SetState(GameState.Result);
                else BeginCountdownForRound(idx);
                break;

            case GameState.Result:
                break;
        }
    }

    void ApplyRoundLocally(MinigameDef def)
    {
        var mine = FindObjectOfType<PunPlayer>();
        if (mine)
        {
            mine.ApplyAbilities(def.abilities);
            mine.ApplyCameraPolicy(def.cameraMode);
        }
    }

    // Util
    int CountHumans() => PhotonNetwork.PlayerList.Length;
    GameState GetState()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(GFKeys.SES_STATE, out var v) &&
            v is byte b) return (GameState)b;
        return GameState.Waiting;
    }
    int GetRoundIndex()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(GFKeys.ROUND_IDX, out var v) &&
            v is int i) return i;
        return 0;
    }
    void SetState(GameState s)
    {
        var rp = new Hashtable { { GFKeys.SES_STATE, (byte)s } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(rp);
    }
    double Remain(string key)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out var v) && v is double end)
            return Mathf.Max(0f, (float)(end - PhotonNetwork.Time));
        return 0;
    }
    MinigameDef GetDef(int roundIdx)
    {
        if (!registry || registry.order == null || registry.order.Length == 0) return null;
        int clamped = Mathf.Clamp(roundIdx, 0, registry.order.Length - 1);
        return registry.order[clamped];
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        UpdateHUD();
        var s = GetState();
        if (s == GameState.Intermission) FinishRound(); // Master側での遷移補助
    }

    void UpdateHUD()
    {
        if (!statusText) return;
        var s = GetState();
        string line = $"State: {s}";
        if (s == GameState.Countdown) line += $"  t={Mathf.CeilToInt((float)Remain(GFKeys.CD_END))}";
        else if (s == GameState.InRound) line += $"  t={Mathf.CeilToInt((float)Remain(GFKeys.ROUND_END))}";
        int humans = PhotonNetwork.PlayerList.Length;
        int bots = botManager ? botManager.CurrentBotCount : 0;
        line += $"  Players {humans + bots} (H{humans}/CPU{bots})";
        statusText.text = line;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        peakHumanCountThisRound = Mathf.Max(peakHumanCountThisRound, CountHumans());
    }
}
