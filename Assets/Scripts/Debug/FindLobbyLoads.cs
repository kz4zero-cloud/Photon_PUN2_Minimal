// Assets/Scripts/Debug/FindLobbyLoads.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Editor 専用のログ検索ツール（MenuItem）
/// ビルドには含まれません。
/// </summary>
public static class FindLobbyLoads
{
    [MenuItem("Tools/Net/Find Lobby Loads (Editor.log)")]
    public static void Run()
    {
        // 任意：ログファイルを選んで簡易検索（必要なら自由に改造してください）
        var defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "game", "デバッグログ");
        var path = EditorUtility.OpenFilePanel("Open log...", Directory.Exists(defaultDir) ? defaultDir : "", "log");
        if (string.IsNullOrEmpty(path)) { Debug.Log("FindLobbyLoads: canceled."); return; }

        int hits = 0;
        foreach (var line in File.ReadLines(path))
        {
            // 気になるキーワードは適宜追加してください
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
// ビルド時は空定義にしてコンパイルを通す
public static class FindLobbyLoads { }
#endif
