// Assets/Scripts/Debug/SpawnDiagnostics.cs
// 目的: 「プレイヤーは生成されているのに見えない」を一括診断
// 実行: 再生中に Tools → Net → Spawn Diagnostics → Run Quick Check
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public static class SpawnDiagnostics
{
    private static readonly string OutDir = @"C:\Users\coupl\Desktop\game\デバッグログ";

    [MenuItem("Tools/Net/Spawn Diagnostics/Run Quick Check")]
    public static void RunQuickCheck()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[SpawnDiag] 再生中に実行してください。");
            return;
        }

        Directory.CreateDirectory(OutDir);
        var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var path = Path.Combine(OutDir, $"SpawnDiag_{ts}.txt");
        var sb = new StringBuilder(4096);

        // ===== 基本情報 =====
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        sb.AppendLine($"timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"scene={scene}");
        sb.AppendLine($"unity={Application.unityVersion}");
        sb.AppendLine();

        // ===== Photon 状態 =====
        try
        {
            var lp = PhotonNetwork.LocalPlayer;
            object cpSpawned = null;
            bool hasCP = lp != null && lp.CustomProperties != null &&
                         lp.CustomProperties.TryGetValue("spawned", out cpSpawned);

            sb.AppendLine("[photon]");
            sb.AppendLine($"connected={PhotonNetwork.IsConnected} ready={PhotonNetwork.IsConnectedAndReady} offline={PhotonNetwork.OfflineMode}");
            sb.AppendLine($"in_room={PhotonNetwork.InRoom} room={PhotonNetwork.CurrentRoom?.Name ?? "-"} players={PhotonNetwork.CurrentRoom?.PlayerCount.ToString() ?? "-"}");
            sb.AppendLine($"local_player={(lp?.NickName ?? "-")}({lp?.ActorNumber.ToString() ?? "-"})");
            sb.AppendLine($"cp.spawned.exists={(hasCP ? "yes" : "no")} value={(cpSpawned ?? "-")}");
            sb.AppendLine();
        }
        catch (Exception e)
        {
            sb.AppendLine("[photon] error=" + e.Message).AppendLine();
        }

        // ===== Prefab の存在 =====
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        sb.AppendLine("[prefab]");
        sb.AppendLine($"resources_found={(playerPrefab ? "yes" : "no")}");
        if (playerPrefab)
        {
            var tag = playerPrefab.tag;
            sb.AppendLine($"prefab_tag={tag}");
            var rends = playerPrefab.GetComponentsInChildren<Renderer>(true);
            sb.AppendLine($"prefab_renderer_count={rends.Length}");
        }
        sb.AppendLine();

        // ===== シーンのカメラ =====
        var cam = Camera.main ?? GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
        sb.AppendLine("[camera]");
        if (cam)
        {
            sb.AppendLine($"name={cam.name} near={cam.nearClipPlane} far={cam.farClipPlane} fov={cam.fieldOfView}");
            sb.AppendLine($"culling_mask={cam.cullingMask}");
        }
        else
        {
            sb.AppendLine("missing_main_camera=YES");
        }
        sb.AppendLine();

        // ===== Spawner と SpawnPoint =====
        var spawner = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                        .FirstOrDefault(mb => mb && mb.GetType().Name == "GamePlayerSpawner");
        sb.AppendLine("[spawner]");
        if (spawner)
        {
            try
            {
                var t = spawner.GetType();
                var dbg = t.GetField("spawnOnStartDebug", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);
                var sps = t.GetField("spawnPoints", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)
                        ?? t.GetField("spawnPoints", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance);

                sb.AppendLine($"spawner_object={spawner.name}");
                if (dbg!=null) sb.AppendLine($"spawnOnStartDebug={dbg.GetValue(spawner)}");

                if (sps!=null)
                {
                    var arr = sps.GetValue(spawner) as System.Collections.IEnumerable;
                    var list = new List<Transform>();
                    if (arr!=null) foreach (var it in arr) if (it is Transform tr) list.Add(tr);
                    sb.AppendLine($"spawnpoints_count={list.Count}");
                    for (int i=0;i<list.Count;i++)
                    {
                        var tr = list[i];
                        sb.AppendLine($" - [{i}] name={tr?.name ?? "-"} pos={FmtV(tr ? tr.position : Vector3.zero)}");
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine("spawner_introspect_error=" + e.Message);
            }
        }
        else sb.AppendLine("spawner_found=NO");
        sb.AppendLine();

        // ===== 自分のプレイヤー候補 =====
        sb.AppendLine("[my_players]");
        var views = GameObject.FindObjectsByType<PhotonView>(FindObjectsSortMode.None);
        var mine = views.Where(pv => pv && pv.IsMine)
                        .Select(pv => pv.gameObject)
                        .Distinct()
                        .ToList();

        sb.AppendLine($"mine_count={mine.Count}");
        for (int i=0;i<mine.Count;i++)
        {
            var go = mine[i];
            sb.AppendLine(DumpGO(go, cam, i));
        }
        if (mine.Count==0) sb.AppendLine("none_found=YES");
        sb.AppendLine();

        // ===== 推定原因（ヒューリスティック）=====
        var reasons = new List<string>();
        if (!PhotonNetwork.InRoom) reasons.Add("NOT_IN_ROOM: 部屋に入っていません（LoadingShellで Ensure Photon Ready=ON を確認）");
        if (!playerPrefab) reasons.Add("PREFAB_MISSING: Resources/Prefabs/Player が見つかりません");
        if (cam == null) reasons.Add("NO_CAMERA_MAIN: Main Camera が見つかりません");

        if (mine.Count == 0 && PhotonNetwork.InRoom && playerPrefab && cam)
        {
            reasons.Add("NO_PLAYER_OBJECT: 生成自体が行われていない可能性（Spawnerの呼び順/条件を確認）");
        }
        else
        {
            // プレイヤーは居るが見えない場合の詳細チェック
            foreach (var go in mine)
            {
                if (go == null) continue;
                if (!go.activeInHierarchy) reasons.Add("INACTIVE: Player GameObject が非アクティブ");
                var rends = go.GetComponentsInChildren<Renderer>(true);
                if (rends.Length == 0) reasons.Add("NO_RENDERER: Renderer がありません");
                else if (!rends.Any(r => r && r.enabled && r.gameObject.activeInHierarchy))
                    reasons.Add("RENDERER_DISABLED: 全Rendererが無効 or 親が非表示");

                if (cam)
                {
                    var b = CombineBounds(rends);
                    if (b.size == Vector3.zero) b = new Bounds(go.transform.position, Vector3.one*0.5f);
                    var inView = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), b);
                    if (!inView) reasons.Add("OFFSCREEN_OR_BEHIND: カメラ視錐に入っていません（位置/向き/距離）");

                    // レイヤーマスク
                    int layer = go.layer;
                    bool culled = (cam.cullingMask & (1 << layer)) == 0;
                    if (culled) reasons.Add($"CULLED_BY_LAYER: カメラのCullingMaskに Layer({LayerMask.LayerToName(layer)}) が含まれていません");
                }

                // スケール0
                var lossy = go.transform.lossyScale;
                if (Mathf.Approximately(lossy.x,0) || Mathf.Approximately(lossy.y,0) || Mathf.Approximately(lossy.z,0))
                    reasons.Add("SCALE_ZERO: スケールが 0 です");
            }
        }

        sb.AppendLine("[suspected_reasons]");
        if (reasons.Count==0) sb.AppendLine("none");
        else foreach (var r in reasons.Distinct()) sb.AppendLine("- " + r);

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        Debug.Log($"[SpawnDiag] 出力: {path}");
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Net/Spawn Diagnostics/Select Candidate")]
    public static void SelectCandidate()
    {
        var go = GameObject.FindObjectsByType<PhotonView>(FindObjectsSortMode.None)
            .Where(pv => pv && pv.IsMine)
            .OrderByDescending(pv => pv.ViewID)
            .Select(pv => pv.gameObject)
            .FirstOrDefault();
        if (go != null)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log($"[SpawnDiag] Selected candidate: {go.name} pos={go.transform.position}");
        }
        else
        {
            Debug.LogWarning("[SpawnDiag] 候補が見つかりませんでした（生成されていない可能性）");
        }
    }

    // ===== Helpers =====
    private static string DumpGO(GameObject go, Camera cam, int idx)
    {
        try
        {
            var sb = new StringBuilder();
            var pos = go.transform.position;
            var lossy = go.transform.lossyScale;
            var rends = go.GetComponentsInChildren<Renderer>(true);
            var b = CombineBounds(rends);
            if (b.size == Vector3.zero) b = new Bounds(go.transform.position, Vector3.one*0.5f);

            sb.AppendLine($"[{idx}] name={go.name} active={go.activeInHierarchy} tag={go.tag} layer={LayerMask.LayerToName(go.layer)}");
            sb.AppendLine($"    pos={FmtV(pos)} rotY={go.transform.eulerAngles.y:F1} scale={FmtV(lossy)}");
            sb.AppendLine($"    renderers={rends.Length} bounds.center={FmtV(b.center)} size={FmtV(b.size)}");

            if (cam)
            {
                var inView = GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), b);
                var vp = cam.WorldToViewportPoint(b.center);
                bool culled = (cam.cullingMask & (1 << go.layer)) == 0;
                sb.AppendLine($"    camera.in_view={inView} viewport={vp} culled_by_layer={culled}");
                sb.AppendLine($"    camera={cam.name} fov={cam.fieldOfView:F1} near={cam.nearClipPlane:F2} far={cam.farClipPlane:F1}");
            }
            return sb.ToString();
        }
        catch (Exception e)
        {
            return $"[{idx}] error_dump_go={e.Message}";
        }
    }

    private static Bounds CombineBounds(Renderer[] rends)
    {
        var valid = rends != null ? rends.Where(r => r && r.enabled && r.gameObject.activeInHierarchy).ToArray() : Array.Empty<Renderer>();
        if (valid.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        var b = valid[0].bounds;
        for (int i=1;i<valid.Length;i++) b.Encapsulate(valid[i].bounds);
        return b;
    }

    private static string FmtV(Vector3 v) => $"({v.x:F2},{v.y:F2},{v.z:F2})";
}
#endif
