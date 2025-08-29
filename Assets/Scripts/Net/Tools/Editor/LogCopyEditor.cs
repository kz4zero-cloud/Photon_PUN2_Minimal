#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LogCopyEditor
{
    // 送り先
    private static readonly string TargetDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    static LogCopyEditor()
    {
        // 再生モード終了時に自動コピー
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CopyAllLogs("Auto(PlayMode end)");
            }
        };
    }

    [MenuItem("Tools/Logs/Copy All Logs to デバッグログ  %#l")] // Ctrl+Shift+L
    public static void CopyAllLogsMenu() => CopyAllLogs("Manual");

    private static void CopyAllLogs(string reason)
    {
        try
        {
            Directory.CreateDirectory(TargetDir);

            // --- Editor.log / Editor-prev.log ---
            var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var editorDir = Path.Combine(localApp, "Unity", "Editor");
            TryCopy(Path.Combine(editorDir, "Editor.log"),
                    Path.Combine(TargetDir, Stamp("Editor") + ".log"));
            TryCopy(Path.Combine(editorDir, "Editor-prev.log"),
                    Path.Combine(TargetDir, Stamp("Editor-prev") + ".log"));

            // --- Player.log（在れば拾う）---
            // Editor上でも Application.persistentDataPath は LocalLow\Company\Product を指す
            var playerLog = Path.Combine(Application.persistentDataPath, "Player.log");
            if (File.Exists(playerLog))
            {
                TryCopy(playerLog,
                        Path.Combine(TargetDir, Stamp("Player") + ".log"));
            }

            Debug.Log($"[LogCopyEditor] {reason}: コピー完了 → {TargetDir}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LogCopyEditor] コピー失敗: {ex.Message}");
        }
    }

    private static string Stamp(string head)
        => $"{head}_{DateTime.Now:yyyyMMdd_HHmmss}";

    private static void TryCopy(string src, string dst)
    {
        if (!File.Exists(src)) return;

        // ログが使用中でも読めるように共有モードで吸い出す
        using (var from = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var to = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            from.CopyTo(to);
        }
    }
}
#endif
