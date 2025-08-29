using System;
using System.IO;
using UnityEngine;

public class LogCopyRuntime : MonoBehaviour
{
    [Tooltip(@"送り先フォルダ（デフォルトは指定のパス）
例: C:\Users\coupl\Desktop\game\デバッグログ")]
    public string targetDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            // 念のためデスクトップ直下にフォールバック
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            targetDir = Path.Combine(desktop, "game", "デバッグログ");
        }
        Directory.CreateDirectory(targetDir);
    }

    private void OnApplicationQuit()
    {
        CopyPlayerLog("Quit");
    }

    // その場で試したいときは F8 でコピー
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
            CopyPlayerLog("F8");
    }

    private void CopyPlayerLog(string reason)
    {
        try
        {
            var playerLog = Path.Combine(Application.persistentDataPath, "Player.log");
            if (!File.Exists(playerLog))
            {
                Debug.Log($"[LogCopyRuntime] Player.log が見つかりません: {playerLog}");
                return;
            }

            var dst = Path.Combine(targetDir, $"Player_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            using (var from = new FileStream(playerLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var to = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                from.CopyTo(to);
            }

            Debug.Log($"[LogCopyRuntime] {reason}: コピー完了 → {dst}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LogCopyRuntime] コピー失敗: {ex.Message}");
        }
    }
}
