using UnityEngine;
using Photon.Pun;

public static class EarlyPrewarm
{
    // 最も早いタイミングで DefaultPool に Player を手動登録
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Run()
    {
        var pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool == null) return;

        // Resources からロード（例: Assets/Resources/Prefabs/Player.prefab）
        var go = Resources.Load<GameObject>("Prefabs/Player");
        if (!go) go = Resources.Load<GameObject>("Player");
        if (!go) { NetLog.Report("Prewarm(Early)", "Player prefab NOT found"); return; }

        // "Player" と "Prefabs/Player" の両キーで登録（どちらで Instantiate しても当たるように）
        if (!pool.ResourceCache.ContainsKey("Player")) pool.ResourceCache["Player"] = go;
        if (!pool.ResourceCache.ContainsKey("Prefabs/Player")) pool.ResourceCache["Prefabs/Player"] = go;

        NetLog.Report("Prewarm(Early)", "Prefabs/Player:OK");
    }
}
