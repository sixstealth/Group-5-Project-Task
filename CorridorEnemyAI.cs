using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType  { SootSprite, AngryToy }
    public enum EnemyState { Drift, InvestigateFlashlight, ReturnToShadow, Chase }

    public EnemyType  enemyType;
    public EnemyState state = EnemyState.Drift;

    public Transform      player;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D      rb;
    private PlayerHealth     playerHP;
    private PlayerVisibility playerVis;

    [Header("Physics Layers (must match Project Settings)")]
    public string enemyBackgroundPhysicsLayer = "EnemyBackground";
    public string enemyForegroundPhysicsLayer = "Enemy";
    private int   _layerBg = -1;
    private int   _layerFg = -1;

    [Header("Sorting Layers")]
    public string backgroundSortingLayer = "Midground";
    public int    backgroundSortingOrder = 0;
    public string foregroundSortingLayer = "Foreground";
    public int    foregroundSortingOrder = 5;

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
    public float searchTimeIfHidden        = 3.5f;

    [Header("Attack")]
    public float attackRange     = 0.9f;
    public float attackCooldown  = 0.85f;
    public int   attackDamage    = 1;
    public float attackKnockback = 4.5f;
    private float attackTimer    = 0f;

    [Header("Hit Stun")]
    public float hitStunTime   = 0.35f;
    private float hitStunTimer = 0f;

    [Header("2.5D Fake Depth")]
    public bool    useFakeDepth         = true;
    public float   backgroundY          = -3.2f;
    public float   depthMoveSpeed       = 2.5f;
    public Vector3 farScale             = new Vector3(0.7f,  0.7f,  1f);
    public Vector3 nearScale            = new Vector3(1.05f, 1.05f, 1f);
    public float   farDistance          = 8f;
    public float   foregroundYThreshold = 0.15f;

    [Header("Swarm Call — SootSprite only")]
    public bool  enableSwarmCall   = true;
    public float swarmCallRadius   = 7f;
    public bool  swarmCallOnlyOnce = true;
    private bool _didSwarmCall     = false;

    private int   driftDir     = 1;
    private float startX;
    private float bobTime      = 0f;    // randomised in Awake so sprites don't pulse in sync
    public  float driftDistance = 2f;

    private bool    hasAlert       = false;
    private Vector2 alertPoint;
    private float   alertWaitTimer = 0f;

    // Prevents re-alerting a sprite that is already confirmed in chase
    private bool combatLocked = false;

    private void Awake()
    {
        _layerBg = LayerMask.NameToLayer(enemyBackgroundPhysicsLayer);
        _layerFg = LayerMask.NameToLayer(enemyForegroundPhysicsLayer);

        // Random phase means a corridor full of sprites won't all bob in unison
        bobTime = Random.Range(0f, 10f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (useFakeDepth)
        {
            transform.position   = new Vector3(transform.position.x, backgroundY, transform.position.z);
            startX               = transform.position.x;
            transform.localScale = farScale;
            SetBackgroundSorting();
            SetBackgroundPhysicsLayer();
        }
        else
        {
            SetForegroundPhysicsLayer();
        }
    }


    public void OnHit() => hitStunTimer = hitStunTime;

    public void OnFlashlightDetected(Transform detectedPlayer)
    {
        if (enemyType == EnemyType.AngryToy) return;
        // Don't interrupt an already-confirmed chase
        if (combatLocked && state == EnemyState.Chase) return;

        player     = detectedPlayer;
        alertPoint = detectedPlayer.position;
        hasAlert   = true;
        combatLocked   = false;

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
            if (useFakeDepth) DoFakeDepth();
            return;
        }

        switch (state)
        {
            case EnemyState.Drift:                 DoDrift();                 break;
            case EnemyState.InvestigateFlashlight: DoInvestigateFlashlight(); break;
            case EnemyState.ReturnToShadow:        DoReturnToShadow();        break;
            case EnemyState.Chase:                 DoChase();                 break;
        }

        if (useFakeDepth) DoFakeDepth();
    }


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
        if (!hasAlert) { state = EnemyState.Drift; return; }

        float dx    = alertPoint.x - transform.position.x;
        float distX = Mathf.Abs(dx);

        if (distX > investigateArriveDistance)
        {
            rb.velocity = new Vector2(Mathf.Sign(dx) * investigateSpeed, rb.velocity.y);
            FlipSprite((int)Mathf.Sign(dx));
            return;
        }

        rb.velocity = new Vector2(0f, rb.velocity.y);

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
        float dx    = startX - transform.position.x;
        float distX = Mathf.Abs(dx);

        if (distX > 0.15f)
        {
            rb.velocity = new Vector2(Mathf.Sign(dx) * driftSpeed, 0f);
            FlipSprite((int)Mathf.Sign(dx));
            return;
        }

        rb.velocity    = Vector2.zero;
        hasAlert       = false;
        alertWaitTimer = 0f;
        combatLocked   = false;
        _didSwarmCall  = false;
        state          = EnemyState.Drift;
    }

    private void DoChase()
    {
        if (player == null) { state = EnemyState.ReturnToShadow; return; }

        float dx      = player.position.x - transform.position.x;
        float distX   = Mathf.Abs(dx);
        float dir     = Mathf.Sign(dx);
        float yDiff   = Mathf.Abs(transform.position.y - player.position.y);
        bool  sameLane = !useFakeDepth || yDiff < foregroundYThreshold * 2f;

        if (distX > attackRange)
            rb.velocity = new Vector2(dir * chaseSpeed, 0f);
        else
        {
            rb.velocity = Vector2.zero;
            if (sameLane && attackTimer <= 0f) { attackTimer = attackCooldown; DoAttack(); }
        }

        FlipSprite((int)dir);
    }


    private void DoFakeDepth()
    {
        bool  wantsFg = state == EnemyState.InvestigateFlashlight || state == EnemyState.Chase;
        float targetY = (wantsFg && player != null) ? player.position.y : backgroundY;

        float newY = Mathf.MoveTowards(transform.position.y, targetY, depthMoveSpeed * Time.fixedDeltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            float t    = Mathf.InverseLerp(farDistance, 0f, dist);
            transform.localScale = Vector3.Lerp(farScale, nearScale, t);
        }

        bool inFg = wantsFg && player != null
                    && Mathf.Abs(transform.position.y - player.position.y) < foregroundYThreshold;

        if (inFg) { SetForegroundSorting(); SetForegroundPhysicsLayer(); }
        else      { SetBackgroundSorting(); SetBackgroundPhysicsLayer();  }
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
            if (Vector2.Distance(transform.position, other.transform.position) > swarmCallRadius) continue;

            other.JoinSwarm(player);
        }
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

    private void SetBackgroundSorting()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sortingLayerName = backgroundSortingLayer;
        spriteRenderer.sortingOrder     = backgroundSortingOrder;
    }

    private void SetForegroundSorting()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sortingLayerName = foregroundSortingLayer;
        spriteRenderer.sortingOrder     = foregroundSortingOrder;
    }

    private void SetBackgroundPhysicsLayer()
    {
        if (_layerBg != -1) SetLayerRecursively(gameObject, _layerBg);
    }

    private void SetForegroundPhysicsLayer()
    {
        if (_layerFg != -1) SetLayerRecursively(gameObject, _layerFg);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnDrawGizmosSelected()
    {
        // Red: melee attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Yellow: last flashlight alert point
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(alertPoint, 0.25f);

        if (enemyType == EnemyType.SootSprite)
        {
            // Purple: swarm broadcast radius
            Gizmos.color = new Color(0.6f, 0f, 1f, 1f);
            Gizmos.DrawWireSphere(transform.position, swarmCallRadius);
        }

        if (enemyType == EnemyType.AngryToy)
        {
            // Orange: proximity aggro range
            Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, toyAggroRange);
        }
    }
}