using UnityEngine;
using UnityEngine.UI;

public class GameProtoReturnBinder : MonoBehaviour
{
    [Tooltip("�߂�{�^���i���w��Ȃ疼�O����: Btn_ReturnToWaiting�j")]
    public Button returnButton;

    [Tooltip("�߂�����i���w��Ȃ�V�[�����玩�����o�j")]
    public GameProtoReturnUI returnUI;

    void Awake()
    {
        if (!returnButton)
        {
            var go = GameObject.Find("Btn_ReturnToWaiting");
            if (go) returnButton = go.GetComponent<Button>();
        }
        if (!returnUI) returnUI = FindObjectOfType<GameProtoReturnUI>();

        if (!returnButton || !returnUI)
        {
            NetLog.Report("ReturnBind", $"missing: button:{(returnButton ? "ok" : "NG")} ui:{(returnUI ? "ok" : "NG")}");
            return;
        }

        returnButton.onClick.AddListener(() =>
        {
            NetLog.Report("ReturnBind", "OnClick -> ReturnToWaiting()");
            returnUI.ReturnToWaiting();
        });

        NetLog.Report("ReturnBind", "Listener added");
    }
}
