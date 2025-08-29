// Assets/Editor/Refactors/DisableExtraSpawns.cs
// メニュー: Tools ▸ Refactors ▸ Disable Extra Spawn Calls
// 目的:
//  1) まだ無効化していない PhotonNetwork.Instantiate(...) を
//     「/* 元コード */ ;」に置換（末尾に必ず空文の ; を挿入して構文を保つ）
//  2) 旧版ツールで「// ...」コメント化した箇所も再整形して「/* ... */ ;」に直す
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class DisableExtraSpawns
{
    private const string ScanRootPrefix = "Assets/Scripts/";
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    // まだ生きている Spawn 呼び出し（; までを1ステートメントとして捕捉）
    private static readonly Regex RxSpawnStmt =
        new Regex(@"PhotonNetwork\.Instantiate\s*\((?:.|\n)*?\)\s*;",
                  RegexOptions.Compiled | RegexOptions.Singleline);

    // 旧ツールで入れた「行コメント版の無効化ブロック」を捕捉
    // 例:
    // // [DISABLED_BY_TOOL] PhotonNetwork.Instantiate was here:
    // // PhotonNetwork.Instantiate(...);
    private static readonly Regex RxOldDisabledBlock =
        new Regex(@"// \[DISABLED_BY_TOOL\] PhotonNetwork\.Instantiate was here:\s*(?:\r?\n//.*)+",
                  RegexOptions.Compiled);

    [MenuItem("Tools/Refactors/Disable Extra Spawn Calls")]
    public static void Run()
    {
        Directory.CreateDirectory(OutputDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var reportPath = Path.Combine(OutputDir, $"Refactor_DisableExtraSpawns_{ts}.txt");
        var report = new List<string>
        {
            $"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            "target=Assets/Scripts/**/*.cs",
            ""
        };

        var scripts = AssetDatabase.FindAssets("t:Script")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Replace('\\','/').StartsWith(ScanRootPrefix, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.EndsWith("/GamePlayerSpawner.cs", StringComparison.OrdinalIgnoreCase)) // 中央スポーナーだけ許可
            .ToList();

        int filesChanged = 0;
        int totalNewDisabled = 0;
        int totalRepairedOld  = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var assetPath in scripts)
            {
                var full = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath).Replace('\\','/');
                if (!File.Exists(full)) continue;

                var src = File.ReadAllText(full, Encoding.UTF8);
                var original = src;

                // 1) 旧コメントブロックを「/* … */ ;」へ修復
                src = RxOldDisabledBlock.Replace(src, m =>
                {
                    var text = m.Value;
                    // 行頭の「// 」を剥がして1つのブロックコメントに詰める
                    var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                    .Select(l => l.StartsWith("// ") ? l.Substring(3) : (l.StartsWith("//") ? l.Substring(2) : l));
                    var body  = string.Join("\n", lines);
                    body = body.Replace("*/", "* /"); // ブロックコメント終端エスケープ
                    return $"/* {body} */ ;";
                });

                // 2) まだ生きている呼び出しを無効化（; を必ず残す）
                int disabledHere = 0;
                src = RxSpawnStmt.Replace(src, m =>
                {
                    disabledHere++;
                    var body = m.Value.Replace("*/", "* /"); // コメント終端の混入を回避
                    return $"/* [DISABLED_BY_TOOL]\n{body}\n*/ ;";
                });
                totalNewDisabled += disabledHere;

                if (!original.Equals(src, StringComparison.Ordinal))
                {
                    // 修復件数の推定（旧ブロック→新ブロック変換の件数）
                    int repaired = RxOldDisabledBlock.Matches(original).Count;
                    totalRepairedOld += repaired;

                    File.WriteAllText(full, src, new UTF8Encoding(false));
                    filesChanged++;
                    report.Add($"{assetPath} : new_disabled={disabledHere}, repaired_old={repaired}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        if (filesChanged == 0) report.Add("(no changes)");
        report.Insert(0, $"files_changed={filesChanged}");
        report.Insert(1, $"total_new_disabled={totalNewDisabled}");
        report.Insert(2, $"total_repaired_old={totalRepairedOld}");

        File.WriteAllLines(reportPath, report, new UTF8Encoding(false));
        Debug.Log($"[DisableExtraSpawns] Report: {reportPath}");
        EditorUtility.RevealInFinder(reportPath);
    }
}
#endif
