// Assets/Scripts/Net/Tools/SceneLoadWatchdog.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SceneLoadWatchdog : MonoBehaviourPunCallbacks
{
    private string logPath;

    public override void OnEnable()
    {
        base.OnEnable();
        logPath = @"C:\Users\coupl\Desktop\game\デバッグログ\scene_loadwatchdog.log";

        Log("#BOOT: SceneLoadWatchdog enabled in scene [" + SceneManager.GetActiveScene().name + "]");
    }

    private void Start()
    {
        Log("#START: SceneLoadWatchdog Start() in scene [" + SceneManager.GetActiveScene().name + "]");
    }

    private void Update()
    {
        // 毎フレーム、現在シーン名と Photon 状態を出すだけ
        string sceneName = SceneManager.GetActiveScene().name;
        Log("#CHECK: Scene=" + sceneName +
            " InRoom=" + PhotonNetwork.InRoom +
            " PlayerCount=" + PhotonNetwork.CurrentRoom?.PlayerCount);
    }

    private void Log(string message)
    {
        try
        {
            File.AppendAllText(logPath, System.DateTime.Now.ToString("HH:mm:ss.fff") + " " + message + "\n");
        }
        catch { }
    }
}
