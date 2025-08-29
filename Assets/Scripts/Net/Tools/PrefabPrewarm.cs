using UnityEngine;

public class PrefabPrewarm : MonoBehaviour
{
    [Tooltip("Resources/ �ȉ��̃v���n�u�p�X�i�g���q�Ȃ��j")]
    [SerializeField] string[] resourcePaths = { "Prefabs/Player" };

    void Awake()
    {
        foreach (var p in resourcePaths)
        {
            var go = Resources.Load<GameObject>(p);
            NetLog.Report("Prewarm", $"{p}:{(go ? "OK" : "MISS")}");
        }
        // ���̃I�u�W�F�N�g�͑S�V�[���Ő�����
        DontDestroyOnLoad(gameObject);
    }
}
