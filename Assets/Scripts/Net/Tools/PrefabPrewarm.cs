using UnityEngine;

public class PrefabPrewarm : MonoBehaviour
{
    [Tooltip("Resources/ 以下のプレハブパス（拡張子なし）")]
    [SerializeField] string[] resourcePaths = { "Prefabs/Player" };

    void Awake()
    {
        foreach (var p in resourcePaths)
        {
            var go = Resources.Load<GameObject>(p);
            NetLog.Report("Prewarm", $"{p}:{(go ? "OK" : "MISS")}");
        }
        // このオブジェクトは全シーンで生かす
        DontDestroyOnLoad(gameObject);
    }
}
