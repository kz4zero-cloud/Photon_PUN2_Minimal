// Assets/Scripts/Debug/RunAssert.cs
// 目的: ランタイム自己採点ログ（Ready/Spawn/Finish/Unload・重複/残骸検知の記録）
using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class RunAssert
{
    private static readonly string OutputDir = @"C:\Users\coupl\Desktop\game\デバッグログ";
    private static readonly object _lock = new object();
    private static string _runId;
    private static string _file;

    public static void Begin(string sceneName)
    {
        Directory.CreateDirectory(OutputDir);
        _runId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _file = Path.Combine(OutputDir, $"RunEval_{_runId}.txt");
        Write($"begin scene={sceneName} product={Application.productName} unity={Application.unityVersion}");
    }

    public static void Mark(string key, string val = "1") => Write($"mark {key}={val}");
    public static void Info(string msg) => Write("info " + msg);
    public static void AssertPass(string name) => Write("assert_pass " + name);
    public static void AssertFail(string name, string detail = "") => Write("assert_fail " + name + (string.IsNullOrEmpty(detail) ? "" : $" :: {detail}"));
    public static void End(string sceneName) => Write($"end scene={sceneName}");

    private static void Write(string line)
    {
        lock (_lock)
        {
            var text = $"{DateTime.Now:HH:mm:ss.fff} {_runId} {line}\n";
            File.AppendAllText(_file, text, new UTF8Encoding(false));
#if UNITY_EDITOR
            Debug.Log("[RunAssert] " + line);
#endif
        }
    }
}
