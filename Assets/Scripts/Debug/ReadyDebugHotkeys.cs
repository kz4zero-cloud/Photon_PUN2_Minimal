using UnityEngine;

public class ReadyDebugHotkeys : MonoBehaviour
{
    public PunStagingFlow flow; // Inspector で GameFlow 自分自身を割当（未指定なら探す）

    private void Reset() { flow = GetComponent<PunStagingFlow>(); }
    private void Awake() { if (!flow) flow = GetComponent<PunStagingFlow>(); }

    private void Update()
    {
        if (!flow) return;

        if (Input.GetKeyDown(KeyCode.F9))
        {
            flow.SetReadyFromUI(true);
            NetLog.Report("ReadyHotkey", "F9 -> True");
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            flow.SetReadyFromUI(false);
            NetLog.Report("ReadyHotkey", "F10 -> False");
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            // 診断スナップショット（PunStagingFlow 側でF8対応済み）
            NetLog.Report("ReadyHotkey", "F8 -> Snapshot request");
        }
    }
}
