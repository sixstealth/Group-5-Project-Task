using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private bool moveTowardPlayer = true;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stoppingDistance = 2.2f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackInterval = 1.5f;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private BossHealth bossHealth;
    private PlayerHealth playerHealth;
    private float attackTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        bossHealth = GetComponent<BossHealth>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
    }

    private void Update()
    {
        if (bossHealth != null && bossHealth.IsDead) return;
        if (player == null) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (moveTowardPlayer && distance > stoppingDistance)
        {
            MoveTowardPlayer();
        }
        else
        {
            StopMoving();
        }

        FacePlayer();

        if (distance <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            AttackPlayer();
        }
    }

    private void MoveTowardPlayer()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(player.position);
            return;
        }

        if (rb == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction.Normalize();
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
    }

    private void StopMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        }
    }
}
