using UnityEngine;

public class ReadyDebugHotkeys : MonoBehaviour
{
    public PunStagingFlow flow; // Inspector �� GameFlow �������g�������i���w��Ȃ�T���j

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
            // �f�f�X�i�b�v�V���b�g�iPunStagingFlow ����F8�Ή��ς݁j
            NetLog.Report("ReadyHotkey", "F8 -> Snapshot request");
        }
    }
}
