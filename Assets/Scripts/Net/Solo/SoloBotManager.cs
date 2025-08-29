using UnityEngine;
using Photon.Pun;

public class SoloBotManager : MonoBehaviour
{
    [SerializeField] string botPrefabPath = "Prefabs/Bot"; // Resources/Prefabs/Bot.prefab
    [SerializeField] Vector2 spawnAreaXZ = new Vector2(6f, 6f);
    [SerializeField] float spawnY = 1.0f;

    void Start()
    {
        if (!(PhotonNetwork.OfflineMode && PunSolo.IsSolo)) return;

        for (int i = 0; i < PunSolo.BotCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaXZ.x, spawnAreaXZ.x),
                spawnY,
                Random.Range(-spawnAreaXZ.y, spawnAreaXZ.y)
            );
            var prefab = Resources.Load<GameObject>(botPrefabPath);
            if (prefab) Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}
