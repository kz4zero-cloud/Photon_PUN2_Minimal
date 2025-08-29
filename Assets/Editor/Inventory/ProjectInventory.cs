// Assets/Editor/Inventory/ProjectInventory.cs
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class ProjectInventory
{
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    [MenuItem("Tools/Inventory/Snapshot")]
    public static void Snapshot()
    {
        Directory.CreateDirectory(OutputDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var path = Path.Combine(OutputDir, $"Inventory_{ts}.txt");

        var sb = new StringBuilder();
        sb.AppendLine($"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"unity_version={Application.unityVersion}");
        sb.AppendLine();

        // Scenes in Build
        var scenesInBuild = EditorBuildSettings.scenes
            .Select(s => new { s.path, s.enabled, name = Path.GetFileName(s.path) })
            .ToList();
        sb.AppendLine("[scenes_in_build]");
        foreach (var s in scenesInBuild)
            sb.AppendLine($"{(s.enabled ? "EN" : "DIS")}\t{s.name}\t{s.path}");
        sb.AppendLine();

        // All scenes under Assets
        var sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
        var scenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        sb.AppendLine("[all_scenes_assets]");
        foreach (var p in scenePaths) sb.AppendLine(p);
        sb.AppendLine();

        // Candidate roles by name heuristic (no assumptions)
        Func<string, bool> isLobby = n => n.IndexOf("lobby", StringComparison.OrdinalIgnoreCase) >= 0;
        Func<string, bool> isMain  = n => n.IndexOf("main",  StringComparison.OrdinalIgnoreCase) >= 0;
        Func<string, bool> isShell = n => n.IndexOf("loadingshell", StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.IndexOf("loading", StringComparison.OrdinalIgnoreCase) >= 0;
        Func<string, bool> isStage = n => n.IndexOf("stage", StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.IndexOf("level", StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.IndexOf("map",   StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.IndexOf("arena", StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.IndexOf("course",StringComparison.OrdinalIgnoreCase) >= 0
                                       || n.StartsWith("Game_", StringComparison.OrdinalIgnoreCase);

        var allSceneNames = scenePaths.Select(Path.GetFileName).ToList();
        sb.AppendLine("[scene_roles_guess]");
        sb.AppendLine("lobby_candidates=" + string.Join(", ", allSceneNames.Where(isLobby)));
        sb.AppendLine("main_candidates="  + string.Join(", ", allSceneNames.Where(isMain)));
        sb.AppendLine("shell_candidates=" + string.Join(", ", allSceneNames.Where(isShell)));
        sb.AppendLine("stage_candidates=" + string.Join(", ", allSceneNames.Where(isStage)));
        sb.AppendLine();

        // Player prefab candidates
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        var prefabs = prefabGuids.Select(AssetDatabase.GUIDToAssetPath);
        var playerPrefabs = prefabs.Where(p =>
            Path.GetFileNameWithoutExtension(p).IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0
            || p.Replace('\\','/').IndexOf("/Prefabs/Player", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        sb.AppendLine("[player_prefab_candidates]");
        foreach (var p in playerPrefabs.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)) sb.AppendLine(p);
        sb.AppendLine();

        // Photon settings presence
        var photonSettings = AssetDatabase.FindAssets("PhotonServerSettings t:ScriptableObject")
            .Select(AssetDatabase.GUIDToAssetPath).ToList();
        sb.AppendLine("[photon]");
        sb.AppendLine("PhotonServerSettings=" + (photonSettings.FirstOrDefault() ?? "(not found)"));
        sb.AppendLine();

        // Code statistics (Assets/Scripts only)
        var scriptGuids = AssetDatabase.FindAssets("t:Script").Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Replace('\\','/').StartsWith("Assets/Scripts/", StringComparison.OrdinalIgnoreCase))
            .ToList();
        int countPunInstantiate = 0, countDDoL = 0, countInstantiateSuspicious = 0;
        var callers = new List<string>();
        var ddolFiles = new List<string>();
        var suspicious = new List<string>();

        foreach (var p in scriptGuids)
        {
            string t;
            try { t = File.ReadAllText(p, Encoding.UTF8); }
            catch { continue; }

            if (t.Contains("PhotonNetwork.Instantiate("))
            { countPunInstantiate++; callers.Add(p); }
            if (t.Contains("DontDestroyOnLoad("))
            { countDDoL++; ddolFiles.Add(p); }
            if (t.Contains("Instantiate(") && (t.Contains("Player") || t.Contains("Prefabs/Player")))
            { countInstantiateSuspicious++; suspicious.Add(p); }
        }
        sb.AppendLine("[code_scan_assets_scripts]");
        sb.AppendLine($"pun_instantiate_calls={countPunInstantiate}");
        callers.ForEach(c => sb.AppendLine(" - " + c));
        sb.AppendLine($"ddol_calls={countDDoL}");
        ddolFiles.ForEach(c => sb.AppendLine(" - " + c));
        sb.AppendLine($"unity_instantiate_player_suspect={countInstantiateSuspicious}");
        suspicious.ForEach(c => sb.AppendLine(" - " + c));

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        Debug.Log($"[Inventory] 出力: {path}");
        EditorUtility.RevealInFinder(path);
    }
}
#endif
