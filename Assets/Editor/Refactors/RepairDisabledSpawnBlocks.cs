// Assets/Editor/Refactors/RepairDisabledSpawnBlocks.cs
// メニュー: Tools ▸ Refactors ▸ Repair Disabled Spawn Blocks
// 目的: if (...) の直後が「/* [DISABLED_BY_TOOL] ... */ ;」だけになって壊れている箇所を
//       自動的に  if (...) { /* … */ ; } へ補修して構文エラーを解消する。
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class RepairDisabledSpawnBlocks
{
    private const string ScanRootPrefix = "Assets/Scripts/";
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    // 例: if (cond) /* [DISABLED_BY_TOOL] ... */ ;
    private static readonly Regex RxIfThenDisabled =
        new Regex(@"if\s*\([^\)]*\)\s*(/\*\s*\[DISABLED_BY_TOOL\][\s\S]*?\*/\s*;)",
                  RegexOptions.Compiled);

    // 旧版（行コメント）のブロックも { } で包む
    // 例:
    // if (cond)
    // // [DISABLED_BY_TOOL] ...
    // // PhotonNetwork.Instantiate(...);
    private static readonly Regex RxIfThenOldDisabled =
        new Regex(@"if\s*\([^\)]*\)\s*((?:\r?\n\s*//.*)+)",
                  RegexOptions.Compiled);

    [MenuItem("Tools/Refactors/Repair Disabled Spawn Blocks")]
    public static void Run()
    {
        Directory.CreateDirectory(OutputDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var reportPath = Path.Combine(OutputDir, $"Refactor_RepairDisabled_{ts}.txt");
        var lines = new List<string>
        {
            $"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            "target=Assets/Scripts/**/*.cs",
            ""
        };

        var scripts = AssetDatabase.FindAssets("t:Script")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Replace('\\','/').StartsWith(ScanRootPrefix, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        int filesChanged = 0, fixedNew = 0, fixedOld = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var assetPath in scripts)
            {
                var full = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath).Replace('\\','/');
                if (!File.Exists(full)) continue;

                var src = File.ReadAllText(full, Encoding.UTF8);
                var original = src;

                // 1) 新方式のブロックを { } で包む
                src = RxIfThenDisabled.Replace(src, m => $"if{ExtractParen(src, m.Index)} {{ {m.Groups[1].Value} }}");

                // 2) 旧行コメントの塊を { } で包み、 /* … */ ; へ変換
                src = RxIfThenOldDisabled.Replace(src, m =>
                {
                    var block = m.Groups[1].Value;
                    if (!block.Contains("[DISABLED_BY_TOOL]")) return m.Value; // 関係ない if はそのまま
                    var body = string.Join("\n", block.Split(new[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(l => l.TrimStart().StartsWith("//") ? l.TrimStart().Substring(2) : l));
                    body = body.Replace("*/", "* /");
                    return $"if{ExtractParen(src, m.Index)} {{ /* [DISABLED_BY_TOOL]\n{body}\n*/ ; }}";
                });

                // 変化があれば保存
                if (!original.Equals(src, StringComparison.Ordinal))
                {
                    // 件数の概算
                    fixedNew += RxIfThenDisabled.Matches(original).Count;
                    fixedOld += RxIfThenOldDisabled.Matches(original).Count;

                    File.WriteAllText(full, src, new UTF8Encoding(false));
                    filesChanged++;
                    lines.Add($"{assetPath} : fixed_if_blocks");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        if (filesChanged == 0) lines.Add("(no changes)");
        lines.Insert(0, $"files_changed={filesChanged}");
        lines.Insert(1, $"fixed_new_if_blocks~={fixedNew}");
        lines.Insert(2, $"fixed_old_if_blocks~={fixedOld}");

        File.WriteAllLines(reportPath, lines, new UTF8Encoding(false));
        Debug.Log($"[RepairDisabledSpawnBlocks] Report: {reportPath}");
        EditorUtility.RevealInFinder(reportPath);
    }

    // m.Index 位置付近から if の括弧部 "(...)" を抽出するヘルパ
    private static string ExtractParen(string source, int matchIndex)
    {
        // 左方向に戻って最も近い '(' を探す
        int i = matchIndex;
        while (i > 0 && source[i] != '(') i--;
        if (i <= 0) return "(/*cond*/)";
        // 右方向に対応する ')' を探す
        int depth = 0;
        int start = i;
        for (int j = i; j < source.Length; j++)
        {
            if (source[j] == '(') depth++;
            else if (source[j] == ')')
            {
                depth--;
                if (depth == 0) return source.Substring(start, j - start + 1);
            }
        }
        return "(/*cond*/)";
    }
}
#endif
