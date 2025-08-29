// Assets/Editor/Refactors/FixDisabledSpawnAssignments.cs
// メニュー: Tools ▸ Refactors ▸ Fix Disabled Spawn Assignments
// 目的: DisableExtraSpawns により「/* [DISABLED_BY_TOOL] … */ ;」へ変換された
//       ステートメントの“代入付き”パターンを安全に修復し、CS0815 等を解消する。
// 対応例:
//   var go = /* [DISABLED_BY_TOOL] ... */ ;          → GameObject go = null; /* ... */ ;
//   GameObject go = /* [DISABLED_BY_TOOL] ... */ ;   → GameObject go = null; /* ... */ ;
//   go = /* [DISABLED_BY_TOOL] ... */ ;              → /* [DISABLED_ASSIGNMENT_REMOVED] go = (spawn) ; */ /* ... */ ;
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class FixDisabledSpawnAssignments
{
    private const string ScanRootPrefix = "Assets/Scripts/";
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    // 「/* [DISABLED_BY_TOOL] … */ ;」ブロック
    private static readonly string DisabledBlock = @"/\*\s*\[DISABLED_BY_TOOL\][\s\S]*?\*/\s*;";

    // 1) var 付き宣言代入  →  GameObject 名前 = null; へ
    private static readonly Regex RxVarDeclAssign = new Regex(
        @"(?m)^(?<indent>\s*)var\s+(?<name>\w+)\s*=\s*(?<block>" + DisabledBlock + @")\s*$",
        RegexOptions.Compiled);

    // 2) 型名付き宣言代入（GameObject など） → 型名はそのまま null へ
    private static readonly Regex RxTypedDeclAssign = new Regex(
        @"(?m)^(?<indent>\s*)(?<type>[A-Za-z_][\w\<\>\.\[\]]*)\s+(?<name>\w+)\s*=\s*(?<block>" + DisabledBlock + @")\s*$",
        RegexOptions.Compiled);

    // 3) 代入のみ（宣言なし） → 丸ごとコメント化（if 直後でも Repair ツールが囲ってくれる）
    private static readonly Regex RxAssignOnly = new Regex(
        @"(?m)^(?<indent>\s*)(?<name>\w+)\s*=\s*(?<block>" + DisabledBlock + @")\s*$",
        RegexOptions.Compiled);

    [MenuItem("Tools/Refactors/Fix Disabled Spawn Assignments")]
    public static void Run()
    {
        Directory.CreateDirectory(OutputDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
        var reportPath = Path.Combine(OutputDir, $"Refactor_FixDisabledAssign_{ts}.txt");
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

        int filesChanged = 0, countVar=0, countTyped=0, countAssign=0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var assetPath in scripts)
            {
                var full = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath).Replace('\\','/');
                if (!File.Exists(full)) continue;

                var src = File.ReadAllText(full, Encoding.UTF8);
                var original = src;

                // 1) var 宣言代入
                src = RxVarDeclAssign.Replace(src, m =>
                    $"{m.Groups["indent"].Value}GameObject {m.Groups["name"].Value} = null; {m.Groups["block"].Value}");
                countVar += RxVarDeclAssign.Matches(original).Count;

                // 2) 型名付き宣言代入
                src = RxTypedDeclAssign.Replace(src, m =>
                    $"{m.Groups["indent"].Value}{m.Groups["type"].Value} {m.Groups["name"].Value} = null; {m.Groups["block"].Value}");
                countTyped += RxTypedDeclAssign.Matches(original).Count;

                // 3) 代入のみ
                src = RxAssignOnly.Replace(src, m =>
                    $"{m.Groups["indent"].Value}/* [DISABLED_ASSIGNMENT_REMOVED] {m.Groups["name"].Value} = (spawn); */ {m.Groups["block"].Value}");
                countAssign += RxAssignOnly.Matches(original).Count;

                if (!original.Equals(src, StringComparison.Ordinal))
                {
                    File.WriteAllText(full, src, new UTF8Encoding(false));
                    filesChanged++;
                    lines.Add($"{assetPath} : fixed var={RxVarDeclAssign.Matches(original).Count}, typed={RxTypedDeclAssign.Matches(original).Count}, assignOnly={RxAssignOnly.Matches(original).Count}");
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
        lines.Insert(1, $"fixed_var_decl={countVar}");
        lines.Insert(2, $"fixed_typed_decl={countTyped}");
        lines.Insert(3, $"fixed_assign_only={countAssign}");

        File.WriteAllLines(reportPath, lines, new UTF8Encoding(false));
        Debug.Log($"[FixDisabledSpawnAssignments] Report: {reportPath}");
        EditorUtility.RevealInFinder(reportPath);
    }
}
#endif
