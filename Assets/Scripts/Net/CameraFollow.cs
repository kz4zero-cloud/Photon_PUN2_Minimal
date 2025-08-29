using System.Linq;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0, 15, -20);
    public float Smooth = 10f;

    void LateUpdate()
    {
        if (Target == null)
        {
            // ���[�J�����L�̃v���C���[���������o
            var me = FindObjectsOfType<PunPlayer>()
                     .FirstOrDefault(p => p.photonView && p.photonView.IsMine);
            if (me != null) Target = me.transform; else return;
        }
        transform.position = Vector3.Lerp(transform.position, Target.position + Offset, Time.deltaTime * Smooth);
        transform.LookAt(Target);
    }
}
