using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NameTag : MonoBehaviour
{
    public float height = 1.4f;   // 頭からの高さ
    public float size = 0.12f;  // 文字の大きさ

    Transform tagTf;
    TextMesh tm;
    PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        // 子オブジェクト生成（存在しなければ）
        var go = new GameObject("NameTag");
        go.transform.SetParent(transform, false);

        tagTf = go.transform;
        tagTf.localPosition = new Vector3(0, height, 0);

        tm = go.AddComponent<TextMesh>();
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = size;
        tm.fontSize = 64;
        tm.color = Color.black;
        tm.text = pv?.Owner != null ? pv.Owner.NickName : "Player";
    }

    void LateUpdate()
    {
        if (!tagTf) return;

        // 位置（万一背が伸びた時でも追従）
        tagTf.localPosition = new Vector3(0, height, 0);

        // カメラへ正対
        var cam = Camera.main;
        if (cam)
        {
            var dir = tagTf.position - cam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                tagTf.rotation = Quaternion.LookRotation(dir);
        }
    }
}
