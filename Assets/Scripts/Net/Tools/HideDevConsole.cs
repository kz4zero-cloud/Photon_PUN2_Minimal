using UnityEngine;

namespace Net.Tools
{
    /// <summary>
    /// Unity�����́uDevelopment Console�v�i�Ԃ������j����ɔ�\���ɂ���B
    /// Development Build �ł��N�����ォ��o�܂���B
    /// </summary>
    public sealed class HideDevConsole : MonoBehaviour
    {
        // �ł��邾�������^�C�~���O�ŏ풓��
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            var go = new GameObject(nameof(HideDevConsole));
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<HideDevConsole>();
        }

        // ���t���[���Ď����ċ����I�ɕ���i�ޯ����Ăŏo����Ă���������j
        private void Update()
        {
            if (Debug.developerConsoleVisible)
                Debug.developerConsoleVisible = false;
        }
    }
}
