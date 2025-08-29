using UnityEngine;

namespace Net.Tools
{
    /// <summary>
    /// Unity内蔵の「Development Console」（赤い小窓）を常に非表示にする。
    /// Development Build でも起動直後から出ません。
    /// </summary>
    public sealed class HideDevConsole : MonoBehaviour
    {
        // できるだけ早いタイミングで常駐化
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject(nameof(HideDevConsole));
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<HideDevConsole>();
        }

        // 毎フレーム監視して強制的に閉じる（ﾊﾞｯｸｸｫｰﾄで出されても即消える）
        private void Update()
        {
            if (Debug.developerConsoleVisible)
                Debug.developerConsoleVisible = false;
        }
    }
}
