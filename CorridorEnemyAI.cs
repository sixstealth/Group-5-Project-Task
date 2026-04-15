using UnityEngine;

public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType  { SootSprite, AngryToy }
    public enum EnemyState { Drift, InvestigateFlashlight, Chase }

    public EnemyType  enemyType;
    public EnemyState state = EnemyState.Drift;

    public Transform      player;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D  rb;
    private PlayerHealth playerHP;

    [Header("Movement")]
    public float driftSpeed    = 1f;
    public float chaseSpeed    = 3f;

    [Header("Toy Aggro")]
    public float toyAggroRange = 2.5f;

    [Header("SootSprite Bob")]
    public bool  verticalBob  = true;
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 1.2f;

    [Header("Flashlight Investigation — SootSprite only")]
    public float investigateSpeed          = 2.8f;
    public float investigateArriveDistance = 0.6f;

    [Header("Attack")]
    public float attackRange     = 0.9f;
    public float attackCooldown  = 0.85f;
    public int   attackDamage    = 1;
    public float attackKnockback = 4.5f;
    private float attackTimer    = 0f;

    [Header("Hit Stun")]
    public float hitStunTime   = 0.35f;
    private float hitStunTimer = 0f;

    private int   driftDir = 1;
    private float startX;
    private float bobTime  = 0f;
    public  float driftDistance = 2f;

    private bool    hasAlert   = false;
    private Vector2 alertPoint;          // world position where the flashlight hit the player

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void OnHit() => hitStunTimer = hitStunTime;

    public void OnFlashlightDetected(Transform detectedPlayer)
    {
        if (enemyType == EnemyType.AngryToy) return;

        player     = detectedPlayer;
        alertPoint = detectedPlayer.position;
        hasAlert   = true;

        state = EnemyState.InvestigateFlashlight;
    }

    // ── Update / FixedUpdate ──────────────────────────────────────────────────

    private void Update()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        if (enemyType == EnemyType.AngryToy && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < toyAggroRange)
                state = EnemyState.Chase;
        }
    }

    private void FixedUpdate()
    {
        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.fixedDeltaTime;
            rb.velocity   = Vector2.zero;
            return;
        }

        switch (state)
        {
            case EnemyState.Drift:                DoDrift();                break;
            case EnemyState.InvestigateFlashlight: DoInvestigateFlashlight(); break;
            case EnemyState.Chase:                DoChase();                break;
        }
    }

    // ── States ────────────────────────────────────────────────────────────────

    private void DoDrift()
    {
        if (transform.position.x > startX + driftDistance) driftDir = -1;
        if (transform.position.x < startX - driftDistance) driftDir =  1;

        float vy = 0f;
        if (enemyType == EnemyType.SootSprite && verticalBob)
        {
            bobTime += Time.fixedDeltaTime;
            vy = Mathf.Sin(bobTime * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        }

        rb.velocity = new Vector2(driftDir * driftSpeed, vy);
        FlipSprite(driftDir);
    }

    private void DoInvestigateFlashlight()
    {
        if (!hasAlert)
        {
            state = EnemyState.Drift;
            return;
        }

        float dx    = alertPoint.x - transform.position.x;
        float distX = Mathf.Abs(dx);

        if (distX > investigateArriveDistance)
        {
            // Still en route to the lit point
            rb.velocity = new Vector2(Mathf.Sign(dx) * investigateSpeed, rb.velocity.y);
            FlipSprite((int)Mathf.Sign(dx));
            return;
        }

        rb.velocity = Vector2.zero;
        state       = EnemyState.Chase;
    }

    private void DoChase()
    {
        if (player == null) return;

        float dx    = player.position.x - transform.position.x;
        float distX = Mathf.Abs(dx);
        float dir   = Mathf.Sign(dx);

        if (distX > attackRange)
            rb.velocity = new Vector2(dir * chaseSpeed, 0f);
        else
        {
            rb.velocity = Vector2.zero;
            if (attackTimer <= 0f) { attackTimer = attackCooldown; DoAttack(); }
        }

        FlipSprite((int)dir);
    }

    private void DoAttack()
    {
        if (playerHP == null && player != null)
            playerHP = player.GetComponent<PlayerHealth>();

        playerHP?.TakeDamage(attackDamage, (Vector2)transform.position, attackKnockback);
    }

    private void FlipSprite(int direction)
    {
        if (spriteRenderer == null || direction == 0) return;
        spriteRenderer.flipX = direction < 0;
    }
}