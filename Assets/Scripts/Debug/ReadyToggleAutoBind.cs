using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ReadyToggleAutoBind : MonoBehaviour
{
    [Tooltip("���w��Ȃ�V�[������ PunStagingFlow ���������o")]
    public PunStagingFlow flow;

    void Awake()
    {
        var t = GetComponent<Toggle>();
        if (!flow) flow = FindObjectOfType<PunStagingFlow>();
        if (!flow)
        {
            NetLog.Report("AutoBind", "PunStagingFlow not found");
            return;
        }

        // ��z�����c���Ă��Ă�OK�B���������I�Ăяo�����Ō�ɒǉ�����
        t.onValueChanged.AddListener((bool v) => {
            flow.SetReadyFromUI(v);
            NetLog.Report("AutoBindDispatch", $"ReadyToggle -> {v}");
        });

        // �N������OFF�ɑ�����iUI�����l�̗h��΍�j
        if (t.isOn)
        {
            t.isOn = false;
            flow.SetReadyFromUI(false);
            NetLog.Report("AutoBindInit", "Force OFF at start");
        }
        else
        {
            NetLog.Report("AutoBindInit", "Start OFF");
        }
    }
}
