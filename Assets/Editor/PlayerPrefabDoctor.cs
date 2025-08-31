// Assets/Editor/PlayerPrefabDoctor.cs
// Tools ▸ Refactors ▸ Verify/Fix Player Prefab
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using Photon.Pun;

public static class PlayerPrefabDoctor
{
    const string PrefabPath = "Assets/Resources/Prefabs/Player.prefab";

    [MenuItem("Tools/Refactors/Verify Player Prefab")]
    public static void Verify()
    {
        if (!System.IO.File.Exists(PrefabPath))
        {
            Debug.LogError($"[PlayerDoctor] Prefab not found: {PrefabPath}");
            return;
        }
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var pv = root.GetComponentInChildren<PhotonView>(true);
            var rends = root.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[PlayerDoctor] PV={(pv? "YES":"NO")}  Tag={root.tag}  Renderers={rends.Length}  AnyRendererEnabled={rends.Any(r=>r && r.enabled)}");
            if (!rends.Any())
                Debug.LogWarning("[PlayerDoctor] Renderer が1つもありません（見えません）。モデル/スプライトをご確認ください。");
            else if (!rends.Any(r=>r && r.enabled))
                Debug.LogWarning("[PlayerDoctor] 全Rendererが無効です。少なくとも1つを有効にしてください。");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Refactors/Fix Player Prefab")]
    public static void Fix()
    {
        if (!System.IO.File.Exists(PrefabPath))
        {
            Debug.LogError($"[PlayerDoctor] Prefab not found: {PrefabPath}");
            return;
        }
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        bool changed = false;
        try
        {
            // 1) Tag を Player に強制
            if (root.tag != "Player") { root.tag = "Player"; changed = true; }

            // 2) PhotonView が無ければ付与（root）
            var pv = root.GetComponent<PhotonView>();
            if (pv == null)
            {
                pv = root.AddComponent<PhotonView>();
                changed = true;
                Debug.Log("[PlayerDoctor] PhotonView を追加しました。");
            }

            // 3) 代表的な Transform 同期（任意だが便利）
            if (root.GetComponent<PhotonTransformView>() == null)
            {
                root.AddComponent<PhotonTransformView>();
                changed = true;
                Debug.Log("[PlayerDoctor] PhotonTransformView を追加しました。");
            }

            // 4) Renderer の存在と有効性をチェック（自動修復はせずに警告）
            var rends = root.GetComponentsInChildren<Renderer>(true);
            if (!rends.Any())
                Debug.LogWarning("[PlayerDoctor] Renderer が1つもありません（見えません）。モデル/スプライトを追加してください。");
            else if (!rends.Any(r=>r && r.enabled))
                Debug.LogWarning("[PlayerDoctor] 全Rendererが無効です。少なくとも1つを有効にしてください。");

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[PlayerDoctor] Player.prefab を保存しました。");
            }
            else
            {
                Debug.Log("[PlayerDoctor] 変更はありません。");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
