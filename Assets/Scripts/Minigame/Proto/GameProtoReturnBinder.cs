using UnityEngine;
using UnityEngine.UI;

public class GameProtoReturnBinder : MonoBehaviour
{
    [Tooltip("戻るボタン（未指定なら名前検索: Btn_ReturnToWaiting）")]
    public Button returnButton;

    [Tooltip("戻る実装（未指定ならシーンから自動検出）")]
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
