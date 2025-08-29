using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NameTag : MonoBehaviour
{
    public float height = 1.4f;   // ������̍���
    public float size = 0.12f;  // �����̑傫��

    Transform tagTf;
    TextMesh tm;
    PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        // �q�I�u�W�F�N�g�����i���݂��Ȃ���΁j
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

        // �ʒu�i����w���L�т����ł��Ǐ]�j
        tagTf.localPosition = new Vector3(0, height, 0);

        // �J�����֐���
        var cam = Camera.main;
        if (cam)
        {
            var dir = tagTf.position - cam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                tagTf.rotation = Quaternion.LookRotation(dir);
        }
    }
}
