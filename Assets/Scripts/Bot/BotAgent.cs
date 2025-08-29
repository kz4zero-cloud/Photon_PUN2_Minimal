using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class BotAgent : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float moveSpeed = 3.8f;
    [SerializeField] float turnSpeed = 7f;
    [SerializeField] float wanderRadius = 8f;
    [SerializeField] float repathInterval = 2.5f;

    [Header("Jump")]
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float jumpCheckAhead = 1.2f;
    [SerializeField] float jumpCooldown = 1.0f;

    [Header("Ground")]
    [SerializeField] LayerMask groundLayers = ~0;
    [SerializeField] float footProbeUpEps = 0.02f;
    [SerializeField] float footProbeRadius = 0.20f;

    Rigidbody rb; CapsuleCollider col;
    Vector3 origin; Vector3 target; float repathTimer; float lastJump = -999f; bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); col = GetComponent<CapsuleCollider>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;
        origin = transform.position;
        PickNewTarget();
    }

    void PickNewTarget()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        target = new Vector3(origin.x + r.x, transform.position.y, origin.z + r.y);
        repathTimer = repathInterval * Random.Range(0.7f, 1.3f);
    }

    void Update()
    {
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f || Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                  new Vector3(target.x, 0, target.z)) < 0.6f) PickNewTarget();
    }

    void FixedUpdate()
    {
        float feetY = col.bounds.min.y + footProbeUpEps;
        Vector3 feet = new Vector3(transform.position.x, feetY, transform.position.z);
        grounded = Physics.OverlapSphere(feet, footProbeRadius, groundLayers, QueryTriggerInteraction.Ignore).Length > 0;
        if (rb.velocity.y > 0.05f) grounded = false;

        Vector3 to = (target - transform.position); to.y = 0;
        if (to.sqrMagnitude > 0.01f)
        {
            var look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.fixedDeltaTime);
        }

        Vector3 vel = rb.velocity;
        Vector3 desired = transform.forward * moveSpeed;
        Vector3 horiz = new Vector3(vel.x, 0, vel.z);
        Vector3 newHoriz = Vector3.Lerp(horiz, new Vector3(desired.x, 0, desired.z), 0.5f);
        rb.velocity = new Vector3(newHoriz.x, vel.y, newHoriz.z);

        if (grounded && Time.time - lastJump >= jumpCooldown)
        {
            if (Physics.Raycast(feet + Vector3.up * 0.1f, transform.forward, out _, jumpCheckAhead))
            {
                vel = rb.velocity; vel.y = 0f; rb.velocity = vel;
                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                lastJump = Time.time;
            }
        }
    }
}
