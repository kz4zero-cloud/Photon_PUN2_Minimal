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

        // まずカプセル高さぶんだけ最低保証
        float targetY = Mathf.Max(transform.position.y, half + extraClearance);

        // 真上から地面へレイキャストして正確に接地（地面が y=0 とは限らない想定）
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
