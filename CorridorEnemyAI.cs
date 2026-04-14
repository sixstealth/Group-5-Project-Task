using UnityEngine;

public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType  { SootSprite, AngryToy }
    public enum EnemyState { Drift, Chase }

    public EnemyType  enemyType;
    public EnemyState state = EnemyState.Drift;

    public Transform      player;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    [Header("Movement")]
    public float driftSpeed    = 1f;
    public float chaseSpeed    = 3f;

    [Header("Toy Aggro")]
    public float toyAggroRange = 2.5f;

    [Header("SootSprite Bob")]
    public bool  verticalBob   = true;    
    public float bobAmplitude  = 0.08f;
    public float bobFrequency  = 1.2f;

    private int   driftDir = 1;
    private float startX;
    private float bobTime  = 0f;
    public  float driftDistance = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (enemyType == EnemyType.AngryToy && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < toyAggroRange)
                state = EnemyState.Chase;
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case EnemyState.Drift: DoDrift(); break;
            case EnemyState.Chase: DoChase(); break;
        }
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

    private void DoChase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dir * chaseSpeed, 0f);
        FlipSprite((int)dir);
    }

    private void FlipSprite(int direction)
    {
        if (spriteRenderer == null || direction == 0) return;
        spriteRenderer.flipX = direction < 0;
    }
}