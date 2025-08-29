using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// �}�X�^�[�����ɑ��삳������UI�p�̃Q�[�g�B
/// �E��}�X�^�[�ɂ͉B�� or ����s�\�ɂ���
/// �E�}�X�^�[�ڏ����ɂ������ŏ�ԍX�V
/// �EConsole�͎g�킸 NetLog �ŋL�^�iNetLog �͊����O��j
///
/// �g����:
///   1) Main�V�[���́u�Q�[���I���v�{�^��(�₻�̐e)�� Add Component
///   2) ��}�X�^�[���̋�����I�ԁiHide / Disable�j
/// </summary>
[DisallowMultipleComponent]
public class MasterOnlyUI : MonoBehaviourPunCallbacks
{
    public enum NonMasterBehavior
    {
        HideGameObject,     // ��}�X�^�[�ɂ�GameObject���܂邲�Ɣ�\��
        DisableInteractable // ��}�X�^�[�ɂ͑���s�\(interactable=false�E������)
    }

    [Header("Target")]
    [Tooltip("���w��Ȃ炱��GameObject�ɓK�p")]
    public GameObject target;

    [Header("Behavior")]
    public NonMasterBehavior behavior = NonMasterBehavior.HideGameObject;

    [Tooltip("Disable���ɑ���s�\�Ƃ���Selectable�Q�i���w��Ȃ�q�K�w���玩�����W�j")]
    public Selectable[] selectables;

    [Tooltip("Disable����CanvasGroup��ǉ����Ď��o�I�ɔ�������")]
    public bool addCanvasGroup = true;

    [Range(0f, 1f)]
    public float disabledAlpha = 0.5f;

    void Awake()
    {
        if (!target) target = gameObject;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Apply("Enable");
    }

    public override void OnJoinedRoom() { Apply("OnJoinedRoom"); }
    public override void OnLeftRoom() { Apply("OnLeftRoom"); }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Apply("OnMasterSwitched");
    }

    void Apply(string tag)
    {
        // InRoom �O�͊m��ł��Ȃ��̂ŕۗ�
        if (!PhotonNetwork.InRoom)
        {
            NetLog.Report("MasterUI", $"Defer {tag} InRoom:false");
            return;
        }

        bool isMaster = PhotonNetwork.IsMasterClient;

        if (behavior == NonMasterBehavior.HideGameObject)
        {
            if (target.activeSelf != isMaster) target.SetActive(isMaster);
            NetLog.Report("MasterUI", isMaster ? "Visible(Master)" : "Hidden(NonMaster)");
            return;
        }

        // DisableInteractable �̏ꍇ
        if (selectables == null || selectables.Length == 0)
            selectables = target.GetComponentsInChildren<Selectable>(true);

        foreach (var s in selectables)
            if (s) s.interactable = isMaster;

        if (addCanvasGroup)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (!cg) cg = target.AddComponent<CanvasGroup>();
            cg.alpha = isMaster ? 1f : disabledAlpha;
            cg.interactable = isMaster;
            cg.blocksRaycasts = isMaster;
        }

        NetLog.Report("MasterUI", isMaster ? "Enabled(Master)" : "Disabled(NonMaster)");
    }
}
