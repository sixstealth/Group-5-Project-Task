using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Animation")]
    [SerializeField] private Animator animator;

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
    private bool hasIsWalkingParameter;
    private bool hasAttackParameter;
    private bool hasDeathParameter;
    private bool deathStarted;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int DeathStateHash = Animator.StringToHash("Base Layer.Death");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        bossHealth = GetComponent<BossHealth>();

        ResolveAnimator();
    }

    private void Start()
    {
        ResolveAnimator();

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
        if (bossHealth != null && bossHealth.IsDead)
        {
            StopMoving();
            if (!deathStarted)
            {
                SetWalking(false);
            }

            return;
        }

        if (player == null)
        {
            SetWalking(false);
            return;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (moveTowardPlayer && distance > stoppingDistance)
        {
            MoveTowardPlayer();
            SetWalking(true);
        }
        else
        {
            StopMoving();
            SetWalking(false);
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
        if (deathStarted || (bossHealth != null && bossHealth.IsDead)) return;

        if (animator != null)
        {
            PlayTriggeredAnimation(AttackHash, "Attack", hasAttackParameter);
        }

        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void PlayDeathAnimation()
    {
        deathStarted = true;
        StopMoving();
        ResolveAnimator();

        if (animator != null)
        {
            if (hasAttackParameter)
            {
                animator.ResetTrigger(AttackHash);
            }

            if (hasDeathParameter)
            {
                animator.ResetTrigger(DeathHash);
            }

            if (hasIsWalkingParameter)
            {
                animator.SetBool(IsWalkingHash, false);
            }

            if (animator.runtimeAnimatorController != null && animator.HasState(0, DeathStateHash))
            {
                animator.Play(DeathStateHash, 0, 0f);
                animator.Update(0f);
            }
            else
            {
                PlayTriggeredAnimation(DeathHash, "Death", hasDeathParameter);
            }
        }
    }

    private void ResolveAnimator()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null) return;

        animator.applyRootMotion = false;
        hasIsWalkingParameter = HasAnimatorParameter(IsWalkingHash);
        hasAttackParameter = HasAnimatorParameter(AttackHash);
        hasDeathParameter = HasAnimatorParameter(DeathHash);
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayTriggeredAnimation(int parameterHash, string stateName, bool hasParameter)
    {
        if (animator.runtimeAnimatorController == null) return;

        if (hasParameter)
        {
            animator.SetTrigger(parameterHash);
            return;
        }

        int stateHash = Animator.StringToHash("Base Layer." + stateName);
        if (animator.HasState(0, stateHash))
        {
            animator.CrossFadeInFixedTime(stateHash, 0.05f);
        }
    }

    private void SetWalking(bool isWalking)
    {
        if (deathStarted) return;
        if (animator == null) return;
        if (animator.runtimeAnimatorController == null) return;

        if (hasIsWalkingParameter)
        {
            animator.SetBool(IsWalkingHash, isWalking);
            return;
        }

        string stateName = isWalking ? "Walk" : "Idle";
        int stateHash = Animator.StringToHash("Base Layer." + stateName);
        if (animator.HasState(0, stateHash))
        {
            animator.CrossFadeInFixedTime(stateHash, 0.1f);
        }
    }
}
