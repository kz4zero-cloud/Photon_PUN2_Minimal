using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Net.Tools
{
    /// <summary>
    /// すでに目的のシーンなら何もしない。違う時だけ PUN 同期ロード。
    /// </summary>
    public static class SceneLoadGate
    {
        public static void LoadLevelIfNeeded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            var current = SceneManager.GetActiveScene().name;
            if (current == sceneName) return;          // 同名ならロードしない

            PhotonNetwork.LoadLevel(sceneName);        // 違う時だけロード
        }
    }
}
