using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerAutoLift : MonoBehaviour
{
    [SerializeField] float extraClearance = 0.01f;
    [SerializeField] LayerMask groundMask = ~0;

    void Start()
    {
        SnapUpToGround("Start");
    }

    void SnapUpToGround(string from)
    {
        var col = GetComponent<CapsuleCollider>();
        float half = (col ? col.height * 0.5f - col.center.y : 0.9f);

        // �܂��J�v�Z�������Ԃ񂾂��Œ�ۏ�
        float targetY = Mathf.Max(transform.position.y, half + extraClearance);

        // �^�ォ��n�ʂփ��C�L���X�g���Đ��m�ɐڒn�i�n�ʂ� y=0 �Ƃ͌���Ȃ��z��j
        var origin = transform.position + Vector3.up * 5f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 20f, groundMask, QueryTriggerInteraction.Ignore))
        {
            targetY = Mathf.Max(targetY, hit.point.y + half + extraClearance);
        }

        var p = transform.position;
        p.y = targetY;
        transform.position = p;
    }
}
