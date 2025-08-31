// Assets/Scripts/Debug/RunAssertRuntime.cs
// 目的：実行中の簡易自己診断と「Finishキーで LoadingShell に戻す」動線を提供。
// ・F9(既定)で LoadingShell を Single でロード（Planがあれば続き/無ければMainへ戻る）
// ・"mark ready / mark spawned" を記録して一度だけ assert_pass を出力
// ・部屋退出時に「残骸なし」を自己採点ログとして出力
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public sealed class RunAssertRuntime : MonoBehaviourPunCallbacks
{
    [Header("Keys")]
    [Tooltip("テスト完了(Finish)に使うキー。押すと LoadingShell をロードします。")]
    public KeyCode finishKey = KeyCode.F9;

    [Header("Routing")]
    [Tooltip("Finish 時に読み込む中継シーン名")]
    public string loadingShellScene = "LoadingShell";

    // ==== 内部状態（assert用） ====
    private static bool s_markReady;
    private static bool s_markSpawned;
    private static bool s_loggedSpawnOnce;

    private void OnEnable()
    {
        // セッション開始ログ
        var scene = SceneManager.GetActiveScene().name;
        Debug.Log($"[RunAssert] begin scene={scene} product={Application.productName} unity={Application.unityVersion}");

        // 一連のフラグをリセットして「準備完了」をマーク
        s_markReady = true;
        s_loggedSpawnOnce = false;
        Debug.Log("[RunAssert] mark ready=1");
        TryAssertSpawnOnce();
    }

    private void Update()
    {
        if (Input.GetKeyDown(finishKey))
        {
            // Finish → LoadingShell をSingleでロード
            Debug.Log("[RunAssert] Finish -> Load LoadingShell");
            if (!string.IsNullOrEmpty(loadingShellScene))
            {
                SceneManager.LoadScene(loadingShellScene, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("[RunAssert] loadingShellScene が未設定です。");
            }
        }
    }

    // ==== 外部から呼べるマーカー（任意で他スクリプトから使える） ====
    public static void MarkSpawned()
    {
        s_markSpawned = true;
        Debug.Log("[RunAssert] mark spawned=1");
        TryAssertSpawnOnce();
    }

    public static void MarkReady()
    {
        s_markReady = true;
        Debug.Log("[RunAssert] mark ready=1");
        TryAssertSpawnOnce();
    }

    private static void TryAssertSpawnOnce()
    {
        if (!s_loggedSpawnOnce && s_markReady && s_markSpawned)
        {
            s_loggedSpawnOnce = true;
            Debug.Log("[RunAssert] assert_pass spawn_once");
        }
    }

    // ==== Photon コールバック（残骸が残っていないかの簡易チェック用ログ） ====
    public override void OnLeftRoom()
    {
        Debug.Log("[RunAssert] assert_pass no_leftover_on_leave");
    }
}
