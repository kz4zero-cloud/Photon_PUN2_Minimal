@echo off
setlocal
set OUTDIR=audit_out
if not exist %OUTDIR% mkdir %OUTDIR%

rem 1) PUN経由でのシーンロード呼び出し
git grep -nE "PhotonNetwork\.(LoadLevel|LoadScene)" > %OUTDIR%\01_pun_load_calls.txt

rem 2) Unity直呼びのシーンロード（AutoSyncと競合しがち）
git grep -nE "SceneManager\.LoadScene" > %OUTDIR%\02_unity_load_calls.txt

rem 3) 起動時に勝手に走るブートストラップ系
git grep -nE "RuntimeInitializeOnLoadMethod|DontDestroyOnLoad" > %OUTDIR%\03_bootstrap_points.txt

rem 4) AutoSyncの設定箇所（複数あるとレース/不統一）
git grep -nE "AutomaticallySyncScene" > %OUTDIR%\04_autosync_writes.txt

rem 5) ルーム/プレイヤーPropsをトリガに動く場所（状態機械の衝突源）
git grep -nE "(OnRoomPropertiesUpdate|OnPlayerPropertiesUpdate)" > %OUTDIR%\05_prop_updaters.txt

rem 6) ローダ/ウォッチャ/デバッグ起動っぽいスクリプト
git grep -nE "AllReadyCountdown|SceneLoadWatchdog|RunAssert|LoadingShell|StageRun|StageSelectUI|ReadyDebugHotkeys|LobbyWhileInRoomGuard|PunBootstrap|PunStagingFlow" > %OUTDIR%\06_loader_like_scripts.txt

rem 7) 直近変更の影響範囲（mainブランチ比較）
git diff --name-only origin/main... > %OUTDIR%\90_changed_files_vs_main.txt

rem 8) 直近の履歴ダイジェスト
git log --oneline --graph -20 > %OUTDIR%\91_recent_log.txt

echo === Done. Outputs under %OUTDIR% ===
