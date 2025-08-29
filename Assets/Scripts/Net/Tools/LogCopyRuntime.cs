using System;
using System.IO;
using UnityEngine;

public class LogCopyRuntime : MonoBehaviour
{
    [Tooltip(@"�����t�H���_�i�f�t�H���g�͎w��̃p�X�j
��: C:\Users\coupl\Desktop\game\�f�o�b�O���O")]
    public string targetDir = @"C:\Users\coupl\Desktop\game\�f�o�b�O���O";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (string.IsNullOrWhiteSpace(targetDir))
        {
            // �O�̂��߃f�X�N�g�b�v�����Ƀt�H�[���o�b�N
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            targetDir = Path.Combine(desktop, "game", "�f�o�b�O���O");
        }
        Directory.CreateDirectory(targetDir);
    }

    private void OnApplicationQuit()
    {
        CopyPlayerLog("Quit");
    }

    // ���̏�Ŏ��������Ƃ��� F8 �ŃR�s�[
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
                Debug.Log($"[LogCopyRuntime] Player.log ��������܂���: {playerLog}");
                return;
            }

            var dst = Path.Combine(targetDir, $"Player_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            using (var from = new FileStream(playerLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var to = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                from.CopyTo(to);
            }

            Debug.Log($"[LogCopyRuntime] {reason}: �R�s�[���� �� {dst}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LogCopyRuntime] �R�s�[���s: {ex.Message}");
        }
    }
}
