using UnityEngine;

public class ForwardUnityLog : MonoBehaviour
{
    void OnEnable()
    {
        Application.logMessageReceived += OnLog;
        DontDestroyOnLoad(gameObject);
    }
    void OnDisable() => Application.logMessageReceived -= OnLog;

    void OnLog(string condition, string stack, LogType type)
    {
        // Photon �̃C���X�^���X�������s�ȂǏd�v�n���E���� netlog ��
        if (condition.Contains("DefaultPool failed to load") ||
            condition.Contains("Instantiate") && condition.Contains("failed") ||
            condition.Contains("client state: Joining"))
        {
            NetLog.Report("UnityLog", condition);
        }
    }
}
