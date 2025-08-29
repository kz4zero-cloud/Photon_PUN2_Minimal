using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace Net.Tools
{
    /// <summary>
    /// �݊��V��: ���� LocalSpawnGate ��c�������R�[�h�����B
    /// �����̓�d�X�|�[���}�~�p�̌y���w���p�[������񋟂��܂��B
    /// </summary>
    public static class LocalSpawnGate
    {
        const string Key = "spawned";

        public static bool AlreadySpawnedMe()
        {
            var me = PhotonNetwork.LocalPlayer;
            return me != null
                && me.CustomProperties != null
                && me.CustomProperties.TryGetValue(Key, out var v)
                && v is bool b && b;
        }

        public static void MarkSpawnedMe(bool value)
        {
            var me = PhotonNetwork.LocalPlayer;
            if (me == null) return;
            var ht = new Hashtable { [Key] = value };
            me.SetCustomProperties(ht);
        }

        /// <summary>�X�|�[�����ɌĂ�: �����̓�d����������~�߂�</summary>
        public static bool CanSpawnMeOnce()
        {
            if (AlreadySpawnedMe())
            {
                NetLog.Report("SkipSpawn(Self) :: Spawned==true");
                return false;
            }
            return true;
        }
    }
}
