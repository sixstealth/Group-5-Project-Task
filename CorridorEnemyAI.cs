using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType  { SootSprite, AngryToy }
    public enum EnemyState { Drift, InvestigateFlashlight, ReturnToShadow, Chase }

    public EnemyType  enemyType;
    public EnemyState state = EnemyState.Drift;

    public Transform player;

    private Rigidbody        rb;
    private PlayerHealth     playerHP;
    private PlayerVisibility playerVis;

    [Header("Movement")]
    public float driftSpeed  = 1.5f;
    public float chaseSpeed  = 3.5f;
    public float driftRadius = 3f; 
    public float rotationSpeed = 5f; 

    [Header("Toy Aggro")]
    public float toyAggroRange = 3f;

    [Header("SootSprite Bob")]
    public bool  verticalBob  = true;
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 1.2f;

    [Header("Flashlight Investigation")]
    public float investigateSpeed          = 2.8f;
    public float investigateArriveDistance = 1.0f;
    public float searchTimeIfHidden        = 3.5f;

    [Header("Attack")]
    public float attackRange     = 1.2f;
    public float attackCooldown  = 0.85f;
    public int   attackDamage    = 1;
    public float attackKnockback = 4.5f;
    private float attackTimer    = 0f;

    [Header("Hit Stun")]
    public float hitStunTime   = 0.35f;
    private float hitStunTimer = 0f;

    [Header("Swarm Call (SootSprite)")]
    public bool  enableSwarmCall   = true;
    public float swarmCallRadius   = 7f;
    public bool  swarmCallOnlyOnce = true;
    private bool _didSwarmCall     = false;

    private Vector3 startPos;
    private Vector3 wanderTarget;
    private float bobTime = 0f;

    private bool    hasAlert       = false;
    private Vector3 alertPoint;
    private float   alertWaitTimer = 0f;

    private bool combatLocked = false;

    private void Awake()
    {
        bobTime = Random.Range(0f, 10f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = (enemyType == EnemyType.AngryToy); 
        rb.freezeRotation = true;

        startPos = transform.position;
        GetNewWanderTarget();
    }

    public void OnHit() => hitStunTimer = hitStunTime;

    public void OnFlashlightDetected(Transform detectedPlayer)
    {
        if (enemyType == EnemyType.AngryToy) return;
        if (combatLocked && state == EnemyState.Chase) return;

        player     = detectedPlayer;
        alertPoint = detectedPlayer.position;
        hasAlert   = true;
        combatLocked = false;

        if (playerVis == null) playerVis = detectedPlayer.GetComponent<PlayerVisibility>();
        if (playerHP  == null) playerHP  = detectedPlayer.GetComponent<PlayerHealth>();

        alertWaitTimer = 0f;
        state          = EnemyState.InvestigateFlashlight;
    }

    public void JoinSwarm(Transform target)
    {
        if (target == null || enemyType == EnemyType.AngryToy) return;
        if (state == EnemyState.Chase) return;

        player = target;
        if (playerVis == null) playerVis = target.GetComponent<PlayerVisibility>();
        if (playerHP  == null) playerHP  = target.GetComponent<PlayerHealth>();

        hasAlert     = true;
        combatLocked = true;
        alertWaitTimer = 0f;
        state          = EnemyState.Chase;
        attackTimer    = 0f;
    }

    private void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        if (enemyType == EnemyType.AngryToy && player != null)
        {
            if (Vector3.Distance(transform.position, player.position) < toyAggroRange)
                state = EnemyState.Chase;
        }
    }

    private void FixedUpdate()
    {
        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.fixedDeltaTime;
            rb.velocity = new Vector3(0, rb.velocity.y, 0); 
            return;
        }

        switch (state)
        {
            case EnemyState.Drift:                 DoDrift();                 break;
            case EnemyState.InvestigateFlashlight: DoInvestigateFlashlight(); break;
            case EnemyState.ReturnToShadow:        DoReturnToShadow();        break;
            case EnemyState.Chase:                 DoChase();                 break;
        }
    }

    private void DoDrift()
    {
        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            GetNewWanderTarget();
        }

        MoveTowardsTarget(wanderTarget, driftSpeed);
    }

    private void GetNewWanderTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * driftRadius;
        wanderTarget = startPos + new Vector3(randomPoint.x, 0f, randomPoint.y);
    }

    private void DoInvestigateFlashlight()
    {
        if (!hasAlert) { state = EnemyState.Drift; return; }

        float distanceToAlert = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                                 new Vector3(alertPoint.x, 0, alertPoint.z));

        if (distanceToAlert > investigateArriveDistance)
        {
            MoveTowardsTarget(alertPoint, investigateSpeed);
            return;
        }

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

        bool playerHid = (playerVis != null) && playerVis.IsHidden;
        if (!playerHid)
        {
            combatLocked = true;
            state        = EnemyState.Chase;
            TriggerSwarmCall();
            return;
        }

        if (alertWaitTimer <= 0f) alertWaitTimer = searchTimeIfHidden;
        alertWaitTimer -= Time.fixedDeltaTime;
        if (alertWaitTimer > 0f) return;

        state = EnemyState.ReturnToShadow;
    }

    private void DoReturnToShadow()
    {
        float distanceToStart = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                                 new Vector3(startPos.x, 0, startPos.z));

        if (distanceToStart > 0.5f)
        {
            MoveTowardsTarget(startPos, driftSpeed);
            return;
        }

        rb.velocity    = new Vector3(0f, rb.velocity.y, 0f);
        hasAlert       = false;
        alertWaitTimer = 0f;
        combatLocked   = false;
        _didSwarmCall  = false;
        GetNewWanderTarget();
        state          = EnemyState.Drift;
    }

    private void DoChase()
    {
        if (player == null) { state = EnemyState.ReturnToShadow; return; }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            MoveTowardsTarget(player.position, chaseSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            FaceDirection((player.position - transform.position).normalized);
            
            if (attackTimer <= 0f) { attackTimer = attackCooldown; DoAttack(); }
        }
    }

    private void MoveTowardsTarget(Vector3 targetPos, float speed)
    {
        Vector3 direction = (targetPos - transform.position);
        direction.y = 0f; 
        direction.Normalize();

        float vy = rb.velocity.y; 


        if (enemyType == EnemyType.SootSprite && verticalBob)
        {
            bobTime += Time.fixedDeltaTime;
            vy = Mathf.Sin(bobTime * bobFrequency * Mathf.PI * 2f) * bobAmplitude * 10f; 
        }

        rb.velocity = new Vector3(direction.x * speed, vy, direction.z * speed);
        FaceDirection(direction);
    }

    private void FaceDirection(Vector3 direction)
    {
        direction.y = 0f; 
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void TriggerSwarmCall()
    {
        if (!enableSwarmCall || (swarmCallOnlyOnce && _didSwarmCall) || player == null) return;

        _didSwarmCall = true;

        foreach (var other in FindObjectsOfType<CorridorEnemyAI>())
        {
            if (other == null || other == this) continue;
            if (other.enemyType == EnemyType.AngryToy) continue;
            if (other.state == EnemyState.Chase) continue;
            if (Vector3.Distance(transform.position, other.transform.position) > swarmCallRadius) continue;

            other.JoinSwarm(player);
        }
    }

    private void DoAttack()
    {
        if (playerHP == null && player != null)
            playerHP = player.GetComponent<PlayerHealth>();

        playerHP?.TakeDamage(attackDamage, transform.position, attackKnockback);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPos : transform.position, driftRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(alertPoint, 0.25f);

        if (enemyType == EnemyType.SootSprite)
        {
            Gizmos.color = new Color(0.6f, 0f, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, swarmCallRadius);
        }

        if (enemyType == EnemyType.AngryToy)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, toyAggroRange);
        }
    }
}