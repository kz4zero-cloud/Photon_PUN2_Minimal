using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoadProbe
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Hook() { SceneManager.sceneLoaded += OnSceneLoaded; }

    static void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        NetLog.Report("SceneLoaded", s.name);
    }
}
