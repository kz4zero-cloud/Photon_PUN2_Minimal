// Assets/Editor/Refactors/NormalizePropertyKeys.cs
// Unity 2022.3+ / メニュー: Tools ▸ Refactors ▸ Normalize Property Keys
// 目的: Assets/Scripts/**/*.cs の「ダブルクォート内の文字列」だけを対象に、
//       Photon Properties のキー表記を正規化（ready/spawned/stageId/runIndex/scoreboard）。
// 変更点: 大小無視比較の辞書で重複キーを登録しない実装に修正（初期化例外の根本解消）。
// 出力:  C:\Users\coupl\Desktop\game\デバッグログ\Refactor_NormPropKeys_YYYYMMDD_HHMM.txt

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal static class NormalizePropertyKeys
{
    private const string ScanRootPrefix = "Assets/Scripts/";
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    // 置換マップ（大小無視比較）。ここには「正規化したい“別名”のみ」を登録する。
    // 正式表記（ready/spawned/stageId/runIndex/scoreboard）そのものは登録しない＝そのまま維持。
    private static readonly Dictionary<string, string> Map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Player
        { "isready",   "ready"   },
        { "isspawned", "spawned" },

        // Room
        { "stageid",   "stageId" },
        { "stage_id",  "stageId" },
        { "runindex",  "runIndex"},
        { "run_index", "runIndex"},
        // 大小ゆれ（scoreBoard）は大小無視の比較で "scoreboard" キーに吸収される
        { "scoreboard","scoreboard"},
    };

    // ダブルクォートの文字列リテラルを拾う（@逐語列は対象外）
    private static readonly Regex RxString = new Regex("(?<!@)\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.Compiled);

    [MenuItem("Tools/Refactors/Normalize Property Keys")]
    public static void Run()
    {
        Directory.CreateDirectory(OutputDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var reportPath = Path.Combine(OutputDir, $"Refactor_NormPropKeys_{ts}.txt");

        var scriptPaths = AssetDatabase.FindAssets("t:Script")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.Replace('\\','/').StartsWith(ScanRootPrefix, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        int filesChanged = 0;
        int totalRewrites = 0;
        var lines = new List<string>
        {
            $"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            "target=Assets/Scripts/**/*.cs",
            ""
        };

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var assetPath in scriptPaths)
            {
                var full = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath).Replace('\\','/');
                if (!File.Exists(full)) continue;

                var src = File.ReadAllText(full, Encoding.UTF8);
                var sb  = new StringBuilder(src.Length + 128);

                int last = 0;
                int rewritesHere = 0;

                foreach (Match m in RxString.Matches(src))
                {
                    // 直前までをそのままコピー
                    sb.Append(src, last, m.Index - last);

                    var withQuotes = m.Value;                // 例: "ready"
                    var inner      = withQuotes.Substring(1, withQuotes.Length - 2); // ready
                    var trimmed    = inner.Trim();

                    // 置換対象か？（大小無視で判定）
                    if (Map.TryGetValue(trimmed, out var canon))
                    {
                        sb.Append('"').Append(canon).Append('"');
                        rewritesHere++;
                    }
                    else
                    {
                        // 正式表記だが前後空白があるケース: "  ready  " など → 詰める
                        if (!ReferenceEquals(inner, trimmed) &&
                            IsFormalKey(trimmed))
                        {
                            sb.Append('"').Append(trimmed).Append('"');
                            rewritesHere++;
                        }
                        else
                        {
                            sb.Append(withQuotes); // 触らない
                        }
                    }

                    last = m.Index + m.Length;
                }

                // 末尾をコピー
                sb.Append(src, last, src.Length - last);

                if (rewritesHere > 0 && !src.Equals(sb.ToString(), StringComparison.Ordinal))
                {
                    File.WriteAllText(full, sb.ToString(), new UTF8Encoding(false));
                    filesChanged++;
                    totalRewrites += rewritesHere;
                    lines.Add($"{assetPath} : {rewritesHere} literal(s) normalized");
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
        lines.Insert(1, $"total_literals_normalized={totalRewrites}");

        File.WriteAllLines(reportPath, lines, new UTF8Encoding(false));
        Debug.Log($"[NormalizePropertyKeys] Report: {reportPath}");
        EditorUtility.RevealInFinder(reportPath);
    }

    private static bool IsFormalKey(string s)
    {
        return s == "ready" || s == "spawned" ||
               s == "stageId" || s == "runIndex" || s == "scoreboard";
    }
}
#endif
