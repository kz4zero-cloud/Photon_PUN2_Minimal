using UnityEngine;
using Photon.Pun;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PunPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("Move")]
    [SerializeField] float baseMoveSpeed = 4.5f;
    [SerializeField] float runMultiplier = 1.6f;
    [SerializeField] float jumpPower = 5.5f;
    [SerializeField, Range(0f, 1f)] float airControl = 0.5f;

    [Header("Keys / Controller")]
    [SerializeField] KeyCode runKeyPrimary = KeyCode.LeftControl;
    [SerializeField] KeyCode runKeySecondary = KeyCode.RightControl;
    [SerializeField] KeyCode toggleNearFarKey = KeyCode.V;

    [Header("Camera")]
    [SerializeField] float camDistanceTPS = 3.2f;
    [SerializeField] float camHeight = 1.6f;
    [SerializeField] float camSmooth = 12f;
    [SerializeField] float orbitSensitivity = 3f;
    [SerializeField] Vector2 pitchLimits = new Vector2(-60f, 70f);

    [Header("Camera Collision")]
    [SerializeField] LayerMask cameraObstacles = ~0;
    [SerializeField] float camSphereRadius = 0.18f;
    [SerializeField] float camSafetyPadding = 0.06f;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayers = ~0;
    [SerializeField] float footProbeUpEps = 0.02f;
    [SerializeField] float footProbeRadius = 0.20f;

    [Header("Jump Guard")]
    [SerializeField] float groundedReadyTime = 0.08f;
    [SerializeField] float jumpCooldown = 0.12f;

    // runtime
    Rigidbody rb; PhotonView pv; CapsuleCollider col;
    Transform camRig; Transform camPivot; Camera mainCam;
    bool isNearView = false; float yaw; float pitch;

    // state
    bool isGrounded; float groundedTimer; float lastJumpTime = -999f; bool jumpPressed;
    Vector3 moveInput;
    AbilityMask abilities = AbilityMask.Default;
    CameraMode cameraMode = CameraMode.FixedFar;

    // --- network sync ---
    Vector3 netPos; Quaternion netRot; Vector3 netVel;
    bool firstNetApplied = false;
    const float REMOTE_POS_LERP = 10f;
    const float REMOTE_ROT_LERP = 12f;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        // ObservedComponents を必ず初期化して自己登録
        if (pv.ObservedComponents == null)
            pv.ObservedComponents = new System.Collections.Generic.List<Component>();
        var me = (Component)this;
        if (!pv.ObservedComponents.Contains(me))
            pv.ObservedComponents.Add(me);
        pv.Synchronization = ViewSynchronization.UnreliableOnChange;

        if (pv.IsMine)
        {
            SetupCameraRig();
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            rb.isKinematic = false;
        }
        else
        {
            // リモートは物理を止め、受信値で再生
            rb.isKinematic = true;
        }
    }

    void SetupCameraRig()
    {
        mainCam = Camera.main;
        if (!mainCam) { Debug.LogWarning("Main Camera not found."); return; }

        camRig = new GameObject("CamRig").transform;
        camRig.SetParent(transform, false);
        camRig.localPosition = new Vector3(0f, camHeight, 0f);

        camPivot = new GameObject("CamPivot").transform;
        camPivot.SetParent(camRig, false);
        camPivot.localRotation = Quaternion.identity;

        mainCam.transform.SetParent(camPivot, false);
        yaw = transform.eulerAngles.y; pitch = 0f;

        ApplyCameraPolicy(cameraMode);
        ApplyAbilities(abilities);
    }

    // ==== API (GameFlowから呼ばれる) ====
    public void ApplyAbilities(AbilityMask mask) { abilities = mask; }
    public void ApplyCameraPolicy(CameraMode mode)
    {
        cameraMode = mode;
        isNearView = (mode == CameraMode.FixedNear);
        if (!mainCam) return;

        mainCam.transform.localRotation = Quaternion.identity;
        Vector3 local = Vector3.zero;
        switch (mode)
        {
            case CameraMode.FixedFar: local = new Vector3(0, 0, -camDistanceTPS); break;
            case CameraMode.FixedNear: local = Vector3.zero; break;
            case CameraMode.OrbitThirdPerson: local = new Vector3(0, 0, -camDistanceTPS); break;
            case CameraMode.FirstPerson: local = Vector3.zero; break;
            case CameraMode.CinematicFollow: local = new Vector3(0, 0, -camDistanceTPS * 0.8f); break;
        }
        mainCam.transform.localPosition = local;
    }

    void Update()
    {
        if (!pv.IsMine) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool runHeld = (Input.GetKey(runKeyPrimary) || Input.GetKey(runKeySecondary) || Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.JoystickButton5));
        moveInput = (transform.forward * v + transform.right * h).normalized
                    * baseMoveSpeed * abilities.maxSpeedMultiplier
                    * (abilities.canRun && runHeld ? runMultiplier : 1f);

        if (abilities.allowNearFarToggle && (Input.GetKeyDown(toggleNearFarKey) || Input.GetKeyDown(KeyCode.JoystickButton3)))
        {
            if (cameraMode == CameraMode.FixedFar || cameraMode == CameraMode.FixedNear)
            {
                isNearView = !isNearView;
                mainCam.transform.localPosition = isNearView ? Vector3.zero : new Vector3(0, 0, -camDistanceTPS);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) jumpPressed = true;

        if (cameraMode == CameraMode.OrbitThirdPerson || cameraMode == CameraMode.FirstPerson)
        {
            float lookX = Input.GetAxis("Mouse X") + Input.GetAxis("LookX");
            float lookY = Input.GetAxis("Mouse Y") + Input.GetAxis("LookY");
            yaw += lookX * orbitSensitivity;
            pitch = Mathf.Clamp(pitch - lookY * orbitSensitivity, pitchLimits.x, pitchLimits.y);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            camPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        if (pv.IsMine)
        {
            float feetY = col.bounds.min.y + footProbeUpEps;
            Vector3 feet = new Vector3(transform.position.x, feetY, transform.position.z);
            bool groundHit = Physics.OverlapSphere(feet, footProbeRadius, groundLayers, QueryTriggerInteraction.Ignore).Any(c => !IsSelf(c));
            if (rb.velocity.y > 0.05f) groundHit = false;

            groundedTimer = groundHit ? groundedTimer + Time.fixedDeltaTime : 0f;
            isGrounded = groundHit;

            Vector3 vel = rb.velocity;
            Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
            Vector3 desired = new Vector3(moveInput.x, 0f, moveInput.z);
            float control = isGrounded ? 1f : Mathf.Clamp01(airControl);
            Vector3 target = Vector3.Lerp(horiz, desired, control);
            rb.velocity = new Vector3(target.x, vel.y, target.z);

            bool canJump = abilities.canJump && isGrounded && groundedTimer >= groundedReadyTime && (Time.time - lastJumpTime) >= jumpCooldown;
            if (jumpPressed && canJump)
            {
                vel = rb.velocity; vel.y = 0f; rb.velocity = vel;
                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                lastJumpTime = Time.time; groundedTimer = 0f;
            }
            jumpPressed = false;
        }
        else
        {
            // 受信値へ補間（指数平滑）
            transform.position = Vector3.Lerp(transform.position, netPos, 1f - Mathf.Exp(-REMOTE_POS_LERP * Time.fixedDeltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, 1f - Mathf.Exp(-REMOTE_ROT_LERP * Time.fixedDeltaTime));
        }
    }

    void LateUpdate()
    {
        if (!pv.IsMine || !mainCam || !camRig) return;

        bool doCollision = (cameraMode == CameraMode.FixedFar || cameraMode == CameraMode.OrbitThirdPerson || cameraMode == CameraMode.CinematicFollow) && !isNearView;
        if (!doCollision) return;

        Vector3 headPos = camRig.position + camPivot.forward * 0.10f;
        Vector3 desiredLocal = new Vector3(0f, 0f, -camDistanceTPS);
        Vector3 desiredWorld = camPivot.TransformPoint(desiredLocal);
        Vector3 dir = (desiredWorld - headPos).normalized;
        float dist = Vector3.Distance(headPos, desiredWorld);

        float safeDist = dist;
        var hits = Physics.SphereCastAll(headPos, camSphereRadius, dir, dist, cameraObstacles, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            var nearest = hits.Where(h => !IsSelf(h.collider)).OrderBy(h => h.distance).FirstOrDefault();
            if (nearest.collider != null) safeDist = Mathf.Max(0.12f, nearest.distance - camSafetyPadding);
        }

        Vector3 current = mainCam.transform.localPosition;
        Vector3 target = new Vector3(0f, 0f, -safeDist);
        mainCam.transform.localPosition = Vector3.Lerp(current, target, 1f - Mathf.Exp(-camSmooth * Time.deltaTime));
    }

    bool IsSelf(Collider c)
    {
        if (!c) return false;
        var t = c.transform;
        return t == transform || t.IsChildOf(transform) || transform.IsChildOf(t) || t.root == transform.root;
    }

    // ====== ネット同期 ======
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
        }
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            netRot = (Quaternion)stream.ReceiveNext();
            netVel = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            netPos += netVel * lag;

            if (!firstNetApplied)
            {
                transform.position = netPos;
                transform.rotation = netRot;
                firstNetApplied = true;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var cc = GetComponent<CapsuleCollider>(); if (!cc) return;
        float feetY = cc.bounds.min.y + footProbeUpEps;
        Vector3 feet = new Vector3(transform.position.x, feetY, transform.position.z);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(feet, footProbeRadius);
    }
#endif
}
