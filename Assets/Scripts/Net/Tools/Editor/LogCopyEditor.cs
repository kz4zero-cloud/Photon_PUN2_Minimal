#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LogCopyEditor
{
    // �����
    private static readonly string TargetDir = @"C:\Users\coupl\Desktop\game\�f�o�b�O���O";

    static LogCopyEditor()
    {
        // �Đ����[�h�I�����Ɏ����R�s�[
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                CopyAllLogs("Auto(PlayMode end)");
            }
        };
    }

    [MenuItem("Tools/Logs/Copy All Logs to �f�o�b�O���O  %#l")] // Ctrl+Shift+L
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

            // --- Player.log�i�݂�ΏE���j---
            // Editor��ł� Application.persistentDataPath �� LocalLow\Company\Product ���w��
            var playerLog = Path.Combine(Application.persistentDataPath, "Player.log");
            if (File.Exists(playerLog))
            {
                TryCopy(playerLog,
                        Path.Combine(TargetDir, Stamp("Player") + ".log"));
            }

            Debug.Log($"[LogCopyEditor] {reason}: �R�s�[���� �� {TargetDir}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LogCopyEditor] �R�s�[���s: {ex.Message}");
        }
    }

    private static string Stamp(string head)
        => $"{head}_{DateTime.Now:yyyyMMdd_HHmmss}";

    private static void TryCopy(string src, string dst)
    {
        if (!File.Exists(src)) return;

        // ���O���g�p���ł��ǂ߂�悤�ɋ��L���[�h�ŋz���o��
        using (var from = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var to = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            from.CopyTo(to);
        }
    }
}
#endif
