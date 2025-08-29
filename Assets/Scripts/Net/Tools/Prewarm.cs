using UnityEngine;
using Photon.Pun;

public static class EarlyPrewarm
{
    // �ł������^�C�~���O�� DefaultPool �� Player ���蓮�o�^
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Run()
    {
        var pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool == null) return;

        // Resources ���烍�[�h�i��: Assets/Resources/Prefabs/Player.prefab�j
        var go = Resources.Load<GameObject>("Prefabs/Player");
        if (!go) go = Resources.Load<GameObject>("Player");
        if (!go) { NetLog.Report("Prewarm(Early)", "Player prefab NOT found"); return; }

        // "Player" �� "Prefabs/Player" �̗��L�[�œo�^�i�ǂ���� Instantiate ���Ă�������悤�Ɂj
        if (!pool.ResourceCache.ContainsKey("Player")) pool.ResourceCache["Player"] = go;
        if (!pool.ResourceCache.ContainsKey("Prefabs/Player")) pool.ResourceCache["Prefabs/Player"] = go;

        NetLog.Report("Prewarm(Early)", "Prefabs/Player:OK");
    }
}
