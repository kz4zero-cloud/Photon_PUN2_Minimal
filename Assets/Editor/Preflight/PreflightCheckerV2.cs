// Assets/Editor/Preflight/PreflightCheckerV2.cs
// Unity 2022.3+ / メニュー: Tools ▸ Preflight ▸ Run All / Open Output Folder
// 目的: 実態に即した“擬陽性の少ない”チェック
//  - スキャンは自前コードのみ: Assets/Scripts/**.cs
//  - 検索は「コメント/文字列を除去」したソースで実施（コメント命中を無視）
//  - single_photon_instantiate: PhotonNetwork.Instantiate が自前コードで厳密に1箇所
//  - no_unity_instantiate_for_player: PhotonNetwork 以外の Instantiate で、近傍に "player" 系があるものだけ検出
//  - no_ddol: DontDestroyOnLoad は原則NG、ただしツール系ホワイトリストは除外
// 出力: C:\Users\coupl\Desktop\game\デバッグログ\Preflight_YYYYMMDD_HHMM.txt

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PreflightV2
{
    internal static class PreflightCheckerV2
    {
        private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

        // DDoL を許容する“ツール系”ファイル（末尾一致で判定）
        private static readonly string[] DDoLWhitelist = new[]
        {
            "/ForwardUnityLog.cs",
            "/HideDevConsole.cs",
            "/LobbyWhileInRoomGuard.cs",
            "/LogCopyRuntime.cs",
            "/PrefabPrewarm.cs",
            "/SceneLoadWatchdog.cs",
        };

        [MenuItem("Tools/Preflight/Run All")]
        public static void RunAll()
        {
            Directory.CreateDirectory(OutputDir);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmm");
            var path = Path.Combine(OutputDir, $"Preflight_{ts}.txt");

            var sb = new StringBuilder();
            sb.AppendLine($"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"unity_version={Application.unityVersion}");
            sb.AppendLine();

            // ===== Scenes In Build（情報）=====
            var scenesInBuild = EditorBuildSettings.scenes
                .Select(s => new { s.path, s.enabled, name = Path.GetFileName(s.path) })
                .ToList();
            sb.AppendLine($"scenes_in_build_enabled_count={scenesInBuild.Count(s=>s.enabled)}");
            sb.AppendLine("[scenes_in_build]");
            foreach (var s in scenesInBuild)
                sb.AppendLine($"{(s.enabled ? "EN" : "DIS")}\t{s.name}\t{s.path}");
            sb.AppendLine();

            // ===== スキャン対象: 自前コードのみ =====
            var scriptPaths = AssetDatabase.FindAssets("t:Script")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.Replace('\\','/').StartsWith("Assets/Scripts/", StringComparison.OrdinalIgnoreCase))
                .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 統計用
            int punInst = 0;
            var punFiles = new List<string>();

            int ddolCount = 0;
            var ddolFiles = new List<string>();

            int instPlayer = 0;
            var instFiles = new List<string>();

            foreach (var assetPath in scriptPaths)
            {
                var full = ToFullPath(assetPath);
                if (string.IsNullOrEmpty(full) || !File.Exists(full)) continue;

                string code;
                try { code = File.ReadAllText(full, Encoding.UTF8); }
                catch { continue; }

                var stripped = StripCommentsAndStrings(code);

                // 1) PhotonNetwork.Instantiate（コメント/文字列除去後）
                if (stripped.Contains("PhotonNetwork.Instantiate("))
                {
                    punInst++;
                    punFiles.Add(assetPath);
                }

                // 2) DontDestroyOnLoad（コメント/文字列除去後）—ホワイトリストは除外
                if (stripped.Contains("DontDestroyOnLoad("))
                {
                    if (!IsWhitelisted(assetPath))
                    {
                        ddolCount++;
                        ddolFiles.Add(assetPath);
                    }
                }

                // 3) Unity.Instantiate のうち “player近傍”だけ検出（PhotonNetworkは除外）
                //    - 最初に候補位置（Instantiate()）を stripped から探す
                //    - 同じインデックス付近のオリジナルコード行に "player" が含まれるかを確認（ゆるく）
                foreach (Match m in Regex.Matches(stripped, @"(?<!PhotonNetwork\.)\bInstantiate\s*\(", RegexOptions.None))
                {
                    // 近傍の元コード行を取得（文字列は含んだままの元テキストから行抽出）
                    var line = GetLineAt(code, m.Index);
                    if (line.IndexOf("player", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.IndexOf("Prefabs/Player", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        instPlayer++;
                        instFiles.Add(assetPath + " :: " + line.Trim());
                    }
                }
            }

            // ===== 判定 =====
            sb.AppendLine($"single_photon_instantiate={(punInst==0 ? "UNKNOWN" : (punInst==1 ? "OK" : "NG"))}");
            if (punInst != 1) foreach (var f in punFiles) sb.AppendLine(" - pun_instantiate_at=" + f);

            sb.AppendLine($"no_ddol={(ddolCount==0 ? "OK" : "NG")}");
            if (ddolCount > 0) foreach (var f in ddolFiles) sb.AppendLine(" - ddol_at=" + f);

            sb.AppendLine($"no_unity_instantiate_for_player={(instPlayer==0 ? "OK" : "NG")}");
            if (instPlayer > 0) foreach (var f in instFiles) sb.AppendLine(" - suspect_instantiate_at=" + f);

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Debug.Log($"[PreflightV2] 出力: {path}");
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("Tools/Preflight/Open Output Folder")]
        public static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDir);
            EditorUtility.RevealInFinder(OutputDir);
        }

        // ===== ヘルパ =====

        private static string ToFullPath(string assetPath)
        {
            try
            {
                var proj = Directory.GetParent(Application.dataPath)!.FullName.Replace('\\','/');
                return Path.Combine(proj, assetPath).Replace('\\','/');
            }
            catch { return null; }
        }

        // 文字列・コメントを除去したテキストを返す（// と /* */、通常の "..." を削る）
        private static string StripCommentsAndStrings(string src)
        {
            var sb = new StringBuilder(src.Length);
            bool inSL = false, inML = false, inSTR = false, esc = false;
            for (int i=0; i<src.Length; i++)
            {
                char c = src[i];
                char n = (i+1 < src.Length) ? src[i+1] : '\0';

                if (inSTR)
                {
                    if (esc) { esc = false; continue; }
                    if (c == '\\') { esc = true; continue; }
                    if (c == '"') { inSTR = false; continue; }
                    continue; // 文字列は出力しない
                }
                if (inSL)
                {
                    if (c == '\n') { inSL = false; sb.Append('\n'); }
                    continue;
                }
                if (inML)
                {
                    if (c=='*' && n=='/') { inML=false; i++; }
                    continue;
                }

                // ここから通常状態
                if (c=='/' && n=='/') { inSL = true; i++; continue; }
                if (c=='/' && n=='*') { inML = true; i++; continue; }
                if (c=='"') { inSTR = true; continue; }

                sb.Append(c);
            }
            return sb.ToString();
        }

        // 指定インデックス付近の元コード“1行”を返す
        private static string GetLineAt(string src, int idx)
        {
            int s = idx;
            while (s > 0 && src[s-1] != '\n') s--;
            int e = idx;
            while (e < src.Length && src[e] != '\n') e++;
            return src.Substring(s, e - s);
        }

        private static bool IsWhitelisted(string assetPath)
        {
            foreach (var tail in DDoLWhitelist)
                if (assetPath.EndsWith(tail, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
#endif
