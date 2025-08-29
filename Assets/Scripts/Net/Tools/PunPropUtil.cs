// Assets/Scripts/Net/Tools/PunPropUtil.cs
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public static class PunPropUtil
{
    public static void SetIfChanged(Player p, string key, object value)
    {
        if (p.CustomProperties != null && p.CustomProperties.TryGetValue(key, out var cur) && Equals(cur, value)) return;
        p.SetCustomProperties(new Hashtable { [key] = value });
    }
}
