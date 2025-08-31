// Assets/Scripts/Debug/FindMyPlayer.cs
#if UNITY_EDITOR
using System.Linq;
using Photon.Pun;
using UnityEditor;
using UnityEngine;

public static class FindMyPlayer
{
    [MenuItem("Tools/Net/Select My Player (Strong)")]
    public static void SelectMineStrong()
    {
        var views = Object.FindObjectsOfType<PhotonView>();
        // 1) 自分が所有しているものを新しい順で
        var mine = views.Where(pv => pv && pv.IsMine)
                        .OrderByDescending(pv => pv.ViewID)
                        .Select(pv => pv.gameObject)
                        .FirstOrDefault();
        // 2) 見つからなければ名前に player を含むもの
        if (mine == null)
            mine = views.Select(pv => pv.gameObject)
                        .FirstOrDefault(go => go && go.name.ToLower().Contains("player"));

        if (mine != null)
        {
            Selection.activeGameObject = mine;
            EditorGUIUtility.PingObject(mine);
            var pos = mine.transform.position;
            Debug.Log($"[FindMyPlayer] Selected: {mine.name} pos={pos} layer={LayerMask.LayerToName(mine.layer)}");
        }
        else
        {
            Debug.LogWarning("[FindMyPlayer] プレイヤーが見つかりません。Spawn失敗 or Renderer/Layer/Tag設定をご確認ください。");
        }
    }
}
#endif
