using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType { SootSprite, AngryToy }
    public enum EnemyState { Drift, InvestigateFlashlight, ReturnToShadow, Chase, Dead }

    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.SootSprite;
    [SerializeField] private EnemyState state = EnemyState.Drift;

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private bool useNavMeshAgentWhenAvailable = true;
    [SerializeField] private float driftSpeed = 1.5f;
    [SerializeField] private float investigateSpeed = 2.8f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float driftRadius = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.6f;
    [SerializeField] private bool keepGrounded = true;

    [Header("Detection")]
    [SerializeField] private bool autoAcquirePlayer = true;
    [SerializeField] private bool chasePlayerInProximity = true;
    [SerializeField] private float proximityAggroRange = 2.5f;
    [SerializeField] private float maxTargetHeightDifference = 2.5f;
    [SerializeField] private float detectionGraceTime = 2f;
    [SerializeField] private float searchTimeIfHidden = 3.5f;
    [SerializeField] private float toyAggroRange = 3f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 0.85f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackKnockback = 4.5f;

    [Header("Soot Sprite Swarm")]
    [SerializeField] private bool enableSwarmCall = true;
    [SerializeField] private float swarmCallRadius = 7f;
    [SerializeField] private bool swarmCallOnlyOnce = true;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private PlayerHealth playerHealth;
    private PlayerHiding playerHiding;
    private PlayerVisibility playerVisibility;

    private Vector3 startPosition;
    private Vector3 wanderTarget;
    private Vector3 alertPoint;
    private float attackTimer;
    private float alertTimer;
    private bool didSwarmCall;

    public EnemyState State => state;
    public bool IsDead => state == EnemyState.Dead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        startPosition = transform.position;
        PickNewWanderTarget();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (keepGrounded)
            {
                rb.constraints |= RigidbodyConstraints.FreezePositionY;
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = attackRange;
            agent.speed = driftSpeed;

            if (!useNavMeshAgentWhenAvailable)
            {
                agent.enabled = false;
            }
        }

        AcquirePlayerIfNeeded();
    }

    private void Update()
    {
        if (state == EnemyState.Dead) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        AcquirePlayerIfNeeded();
        TryProximityChase();

        switch (state)
        {
            case EnemyState.Drift:
                DoDrift();
                break;
            case EnemyState.InvestigateFlashlight:
                DoInvestigateFlashlight();
                break;
            case EnemyState.ReturnToShadow:
                DoReturnToShadow();
                break;
            case EnemyState.Chase:
                DoChase();
                break;
        }
    }

    public void OnFlashlightDetected(Transform detectedPlayer)
    {
        if (state == EnemyState.Dead || detectedPlayer == null) return;

        CachePlayer(detectedPlayer);
        alertPoint = detectedPlayer.position;

        if (IsPlayerHidden() || !IsTargetAtTrackableHeight(player))
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        alertTimer = detectionGraceTime;
        state = EnemyState.InvestigateFlashlight;
    }

    public void BeginChase(Transform target)
    {
        if (state == EnemyState.Dead || target == null) return;

        CachePlayer(target);

        if (IsPlayerHidden() || !IsTargetAtTrackableHeight(player))
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        state = EnemyState.Chase;
        alertTimer = 0f;
        TriggerSwarmCall();
    }

    public void JoinSwarm(Transform target)
    {
        if (enemyType == EnemyType.AngryToy || state == EnemyState.Dead) return;
        BeginChase(target);
    }

    public void OnPlayerHidden(Transform hiddenPlayer)
    {
        if (state == EnemyState.Dead || player == null || hiddenPlayer != player) return;

        if (state == EnemyState.Chase || state == EnemyState.InvestigateFlashlight)
        {
            alertPoint = player.position;
            alertTimer = searchTimeIfHidden;
            state = EnemyState.ReturnToShadow;
        }
    }

    public void Die()
    {
        if (state == EnemyState.Dead) return;

        state = EnemyState.Dead;
        StopMoving();

        if (agent != null)
        {
            agent.enabled = false;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider enemyCollider in colliders)
        {
            enemyCollider.enabled = false;
        }

        Destroy(gameObject, 0.2f);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryContactAttack(collision.collider);
    }

    private void DoDrift()
    {
        if (Vector3.Distance(transform.position, wanderTarget) <= arriveDistance)
        {
            PickNewWanderTarget();
        }

        MoveTo(wanderTarget, driftSpeed);
    }

    private void DoInvestigateFlashlight()
    {
        if (player == null)
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        if (IsPlayerHidden() || !IsTargetAtTrackableHeight(player))
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        alertPoint = player.position;
        MoveTo(alertPoint, investigateSpeed);

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
        {
            BeginChase(player);
        }
    }

    private void DoReturnToShadow()
    {
        if (Vector3.Distance(transform.position, startPosition) > arriveDistance)
        {
            MoveTo(startPosition, driftSpeed);
            return;
        }

        StopMoving();
        didSwarmCall = false;
        PickNewWanderTarget();
        state = EnemyState.Drift;
    }

    private void DoChase()
    {
        if (player == null)
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        if (IsPlayerHidden() || !IsTargetAtTrackableHeight(player))
        {
            state = EnemyState.ReturnToShadow;
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            MoveTo(player.position, chaseSpeed);
            return;
        }

        StopMoving();
        FaceDirection(player.position - transform.position);

        TryAttackIfReady();
    }

    private void CachePlayer(Transform target)
    {
        PlayerHealth health = target.GetComponentInParent<PlayerHealth>();
        Transform playerTransform = health != null ? health.transform : target;

        player = playerTransform;
        playerHealth = health != null ? health : playerTransform.GetComponent<PlayerHealth>();
        playerHiding = playerTransform.GetComponent<PlayerHiding>();
        playerVisibility = playerTransform.GetComponent<PlayerVisibility>();
    }

    private void AcquirePlayerIfNeeded()
    {
        if (!autoAcquirePlayer || player != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            CachePlayer(playerObject.transform);
        }
    }

    private void TryProximityChase()
    {
        if (player == null || IsPlayerHidden() || !IsTargetAtTrackableHeight(player)) return;
        if (state == EnemyState.Chase || state == EnemyState.ReturnToShadow) return;

        float aggroRange = enemyType == EnemyType.AngryToy ? toyAggroRange : proximityAggroRange;
        if (enemyType != EnemyType.AngryToy && !chasePlayerInProximity) return;

        if (Vector3.Distance(transform.position, player.position) <= aggroRange)
        {
            BeginChase(player);
        }
    }

    private void TryContactAttack(Collider other)
    {
        if (state == EnemyState.Dead || other == null) return;
        if (!other.CompareTag("Player")) return;

        CachePlayer(other.transform);
        if (IsPlayerHidden()) return;

        if (state != EnemyState.Chase)
        {
            BeginChase(player);
        }

        if (player == null) return;
        if (Vector3.Distance(transform.position, player.position) > attackRange + 0.35f) return;

        StopMoving();
        FaceDirection(player.position - transform.position);
        TryAttackIfReady();
    }

    private bool IsPlayerHidden()
    {
        if (playerHiding != null) return playerHiding.isHiding;
        if (playerVisibility != null) return playerVisibility.IsHidden;
        return false;
    }

    private bool IsTargetAtTrackableHeight(Transform target)
    {
        if (target == null) return false;

        return Mathf.Abs(target.position.y - transform.position.y) <= maxTargetHeightDifference;
    }

    private void TryAttackIfReady()
    {
        if (attackTimer > 0f) return;

        attackTimer = attackCooldown;
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            ApplyKnockbackToPlayer();
        }
    }

    private void ApplyKnockbackToPlayer()
    {
        if (playerHealth == null || attackKnockback <= 0f) return;

        Rigidbody playerRigidbody = playerHealth.GetComponent<Rigidbody>();
        if (playerRigidbody == null) return;

        Vector3 pushDirection = (playerHealth.transform.position - transform.position).normalized;
        pushDirection.y = 0.5f;
        pushDirection.Normalize();
        playerRigidbody.AddForce(pushDirection * attackKnockback, ForceMode.Impulse);
    }

    private void MoveTo(Vector3 targetPosition, float speed)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh && useNavMeshAgentWhenAvailable)
        {
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(targetPosition);
            return;
        }

        if (rb == null) return;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            StopMoving();
            return;
        }

        direction.Normalize();
        float verticalVelocity = keepGrounded ? 0f : rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(direction.x * speed, verticalVelocity, direction.z * speed);
        FaceDirection(direction);
    }

    private void StopMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (rb != null)
        {
            float verticalVelocity = keepGrounded ? 0f : rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(0f, verticalVelocity, 0f);
        }
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * driftRadius;
        wanderTarget = startPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);
    }

    private void TriggerSwarmCall()
    {
        if (!enableSwarmCall || player == null) return;
        if (swarmCallOnlyOnce && didSwarmCall) return;

        didSwarmCall = true;

        CorridorEnemyAI[] enemies = FindObjectsOfType<CorridorEnemyAI>();
        foreach (CorridorEnemyAI other in enemies)
        {
            if (other == null || other == this) continue;
            if (Vector3.Distance(transform.position, other.transform.position) > swarmCallRadius) continue;

            other.JoinSwarm(player);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, driftRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? alertPoint : transform.position, arriveDistance);

        Gizmos.color = new Color(0.6f, 0f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, swarmCallRadius);
    }
}
