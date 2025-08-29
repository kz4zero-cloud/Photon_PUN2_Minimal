// Assets/Scripts/Debug/FindLobbyLoads.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Editor ��p�̃��O�����c�[���iMenuItem�j
/// �r���h�ɂ͊܂܂�܂���B
/// </summary>
public static class FindLobbyLoads
{
    [MenuItem("Tools/Net/Find Lobby Loads (Editor.log)")]
    public static void Run()
    {
        // �C�ӁF���O�t�@�C����I��ŊȈՌ����i�K�v�Ȃ玩�R�ɉ������Ă��������j
        var defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "game", "�f�o�b�O���O");
        var path = EditorUtility.OpenFilePanel("Open log...", Directory.Exists(defaultDir) ? defaultDir : "", "log");
        if (string.IsNullOrEmpty(path)) { Debug.Log("FindLobbyLoads: canceled."); return; }

        int hits = 0;
        foreach (var line in File.ReadLines(path))
        {
            // �C�ɂȂ�L�[���[�h�͓K�X�ǉ����Ă�������
            if (line.Contains("SceneLoaded:: Lobby") ||
                line.Contains("Back To Lobby") ||
                line.Contains("LoadLevel") ||
                line.Contains("RoomClosed"))
            {
                Debug.Log(line);
                hits++;
            }
        }
        Debug.Log($"FindLobbyLoads: {hits} hits in {path}");
    }
}
#else
// �r���h���͋��`�ɂ��ăR���p�C����ʂ�
public static class FindLobbyLoads { }
#endif
