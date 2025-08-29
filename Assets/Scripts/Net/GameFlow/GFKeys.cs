// �i�s�ƃX�R�A�̃L�[���ЂƂ܂Ƃ߂ɂ�����ʌ݊��Z�b�g�B
// �u�ȈՔŁiGFHud / PunGameFlow1�j�v�Ɓu���@�\�ŁiPunGameFlow�j�v�̗����{ScoreService�Ŏg���܂��B

// --- �ȈՔłŎg����� ---
public enum GFState : byte
{
    Waiting = 0,  // �ҋ@
    Countdown = 1,  // �J�E���g�_�E���i�J�n����=t0�j
    Playing = 2,  // �i�s���i�I������=t0�j
    Results = 3,  // ���U���g/�C���^�[�o���i���A����=t0�j
}

// --- ���@�\�łŎg����� ---
public enum GameState : byte
{
    Waiting = 0,
    RoundReady = 1,
    Countdown = 2,
    InRound = 3,
    Intermission = 4,
    Result = 5,
}

// --- ���@�\�ŁF�J�n�|���V�[ ---
public enum StartPolicy : byte
{
    HostForce = 0, // �z�X�g���J�n
    AllReady = 1, // �S�����f�B�ŊJ�n
    MinPlayersWait = 2, // �l���������l�ŊJ�n
}

// --- ���L�L�[�Q�iRoom/Player CustomProperties �� RaiseEvent �R�[�h�j ---
public static class GFKeys
{
    // �ȈՔŁiGFHud / PunGameFlow1�j
    public const string STATE = "gf_state";   // byte(GFState)
    public const string ROUND = "gf_round";   // int
    public const string MINI = "gf_mini";    // int
    public const string T0 = "gf_t0";      // double

    // ���@�\�ŁiPunGameFlow�j
    public const string SES_STATE = "ses_state";        // byte(GameState)
    public const string ROUND_IDX = "round_idx";        // int
    public const string MG_KEY = "mg_key";           // string
    public const string SEED = "seed";             // int
    public const string CD_END = "cd_end";           // double
    public const string ROUND_END = "round_end";        // double
    public const string START_POLICY = "start_policy";     // byte(StartPolicy)
    public const string DESIRED_PLAYERS = "desired_players";  // int

    // �X�R�A�iScoreService �p�j
    // Player CustomProperties �ɓ����l�X�R�A�̃L�[
    public const string SCORE = "p_score";                    // int
    // RaiseEvent �̃A�v���p�R�[�h�iPhoton�̐����ɏ]�� 200+ ���g�p�j
    public const byte EV_SCORE_DELTA = 201; // ���e: (int actorNumber, int delta)
    public const byte EV_SCORE_SET = 202; // ���e: (int actorNumber, int value)
    public const byte EV_SCORE_RESET_ALL = 203; // �S���Z�b�g�ȂǕK�v�Ȃ�
}
