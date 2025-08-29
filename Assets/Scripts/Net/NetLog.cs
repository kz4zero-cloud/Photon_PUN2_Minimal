using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class NetLog
{
    private static readonly string Dir = @"C:\Users\coupl\Desktop\game\デバッグログ";
    private static readonly string FilePath;

    // 直近ログの去来抑制
    static readonly Dictionary<string, float> lastByKey = new Dictionary<string, float>(256);
    static readonly Dictionary<string, object> lastValue = new Dictionary<string, object>(128);

    static NetLog()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            FilePath = Path.Combine(Dir, $"netlog_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            File.AppendAllText(FilePath, $"=== START {DateTime.Now:yyyy/MM/dd HH:mm:ss} ===\n", Encoding.UTF8);
        }
        catch { /* ignore */ }
    }

    /// いつものログ（既存呼び出しは全部これのままでOK）
    public static void Report(string reason, string details = "")
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {reason}{(string.IsNullOrEmpty(details) ? "" : " :: " + details)}\n";
        try { File.AppendAllText(FilePath, line, Encoding.UTF8); } catch { /* ignore */ }
    }

    /// 一定時間内の同一行を抑制（デフォルト0.25秒）
    public static void ReportDedup(string reason, string details = "", float dedupSeconds = 0.25f)
    {
        string key = reason + "|" + details;
        float now = Time.unscaledTime;
        if (lastByKey.TryGetValue(key, out var t) && now - t < dedupSeconds) return;
        lastByKey[key] = now;
        Report(reason, details);
    }

    /// 値が変化した時だけ出す（キーごと）
    public static void ReportChanged<T>(string key, T value, string where = "")
    {
        if (lastValue.TryGetValue(key, out var cur) && Equals(cur, value)) return;
        lastValue[key] = value;
        Report(key, (where == "" ? "" : where + " => ") + (value?.ToString() ?? "null"));
    }

    /// 一度だけ（同キーで二回目以降は無視）
    public static void ReportOnce(string key, string details = "")
    {
        if (lastByKey.ContainsKey("ONCE|" + key)) return;
        lastByKey["ONCE|" + key] = Time.unscaledTime;
        Report(key, details);
    }
}
