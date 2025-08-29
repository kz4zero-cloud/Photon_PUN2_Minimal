using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleWireProbe : MonoBehaviour
{
    private Toggle t;

    private void Awake()
    {
        t = GetComponent<Toggle>();
        Dump("Awake");
        t.onValueChanged.AddListener(OnChanged);
    }

    private void OnDestroy()
    {
        if (t != null) t.onValueChanged.RemoveListener(OnChanged);
    }

    private void OnChanged(bool v)
    {
        NetLog.Report("ToggleChanged", $"{name}.isOn -> {v}");
        Dump("OnChanged");
    }

    private void Start() => Dump("Start");

    private void Dump(string tag)
    {
        int count = t.onValueChanged.GetPersistentEventCount();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < count; i++)
        {
            UnityEngine.Object target = t.onValueChanged.GetPersistentTarget(i);
            string method = t.onValueChanged.GetPersistentMethodName(i);
            sb.Append($"{i}:{(target ? target.name : "(null)")}#{method} ");
        }
        NetLog.Report(
            "ToggleProbe",
            $"{tag} isOn:{t.isOn} persistent:{count} [{sb}]"
        );
    }
}
