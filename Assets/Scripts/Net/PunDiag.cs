using UnityEngine;
using Photon.Pun;

public static class PunDiag
{
    public static void CheckPrefab()
    {
        var prefab = Resources.Load<GameObject>("Prefabs/Player");
        if (prefab == null)
        {
            Debug.LogError("[PunDiag] Resources.Load(\"Prefabs/Player\") = NULL（パス/配置ミス）");
            return;
        }
        var pv = prefab.GetComponent<PhotonView>();
        var rb = prefab.GetComponent<Rigidbody>();
        var pp = prefab.GetComponent<PunPlayer>();
        Debug.Log($"[PunDiag] Prefab comps pv:{(pv ? "OK" : "MISSING")} rb:{(rb ? "OK" : "MISSING")} punPlayer:{(pp ? "OK" : "MISSING")}");
    }

    public static void ScanScene()
    {
        var list = Object.FindObjectsOfType<PunPlayer>(true);
        foreach (var p in list) Debug.Log($"[PunDiag] Scene has PunPlayer on: {p.gameObject.name}");
        if (list.Length == 0) Debug.Log("[PunDiag] Scene has no PunPlayer (OK) 生成時に付く想定");
    }

    public static void DumpSpawn(GameObject go)
    {
        if (go == null) { Debug.LogError("[PunDiag] DumpSpawn: go is null"); return; }
        var pv = go.GetComponent<PhotonView>();
        var rb = go.GetComponent<Rigidbody>();
        var pp = go.GetComponent<PunPlayer>();
        Debug.Log($"[PunDiag] Spawned: name={go.name}, pv:{(pv ? "OK" : "MISSING")} isMine:{(pv ? pv.IsMine : false)}, rb:{(rb ? "OK" : "MISSING")}, punPlayer:{(pp ? "OK" : "MISSING")}");
        if (rb)
        {
            var c = rb.constraints;
            Debug.Log($"[PunDiag] RB kinematic={rb.isKinematic}, FreezeRotX={c.HasFlag(RigidbodyConstraints.FreezeRotationX)}, FreezeRotZ={c.HasFlag(RigidbodyConstraints.FreezeRotationZ)}");
        }
    }
}
