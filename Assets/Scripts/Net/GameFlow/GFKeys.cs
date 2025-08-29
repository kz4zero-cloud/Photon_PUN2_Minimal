// 進行とスコアのキーをひとまとめにした上位互換セット。
// 「簡易版（GFHud / PunGameFlow1）」と「高機能版（PunGameFlow）」の両方＋ScoreServiceで使えます。

// --- 簡易版で使う状態 ---
public enum GFState : byte
{
    Waiting = 0,  // 待機
    Countdown = 1,  // カウントダウン（開始時刻=t0）
    Playing = 2,  // 進行中（終了時刻=t0）
    Results = 3,  // リザルト/インターバル（復帰時刻=t0）
}

// --- 高機能版で使う状態 ---
public enum GameState : byte
{
    Waiting = 0,
    RoundReady = 1,
    Countdown = 2,
    InRound = 3,
    Intermission = 4,
    Result = 5,
}

// --- 高機能版：開始ポリシー ---
public enum StartPolicy : byte
{
    HostForce = 0, // ホストが開始
    AllReady = 1, // 全員レディで開始
    MinPlayersWait = 2, // 人数しきい値で開始
}

// --- 共有キー群（Room/Player CustomProperties と RaiseEvent コード） ---
public static class GFKeys
{
    // 簡易版（GFHud / PunGameFlow1）
    public const string STATE = "gf_state";   // byte(GFState)
    public const string ROUND = "gf_round";   // int
    public const string MINI = "gf_mini";    // int
    public const string T0 = "gf_t0";      // double

    // 高機能版（PunGameFlow）
    public const string SES_STATE = "ses_state";        // byte(GameState)
    public const string ROUND_IDX = "round_idx";        // int
    public const string MG_KEY = "mg_key";           // string
    public const string SEED = "seed";             // int
    public const string CD_END = "cd_end";           // double
    public const string ROUND_END = "round_end";        // double
    public const string START_POLICY = "start_policy";     // byte(StartPolicy)
    public const string DESIRED_PLAYERS = "desired_players";  // int

    // スコア（ScoreService 用）
    // Player CustomProperties に入れる個人スコアのキー
    public const string SCORE = "p_score";                    // int
    // RaiseEvent のアプリ用コード（Photonの推奨に従い 200+ を使用）
    public const byte EV_SCORE_DELTA = 201; // 内容: (int actorNumber, int delta)
    public const byte EV_SCORE_SET = 202; // 内容: (int actorNumber, int value)
    public const byte EV_SCORE_RESET_ALL = 203; // 全リセットなど必要なら
}
