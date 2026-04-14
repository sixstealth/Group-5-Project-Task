using UnityEngine;

public class CorridorEnemyAI : MonoBehaviour
{
    public enum EnemyType  { SootSprite, AngryToy }
    public enum EnemyState { Drift, Chase }

    public EnemyType  enemyType;
    public EnemyState state = EnemyState.Drift;

    public Transform      player;
    public SpriteRenderer spriteRenderer;   // assign in Inspector or auto-found in Start

    private Rigidbody2D rb;

    [Header("Movement")]
    public float driftSpeed    = 1f;
    public float chaseSpeed    = 3f;

    [Header("Toy Aggro")]
    public float toyAggroRange = 2.5f;

    private int   driftDir     = 1;
    private float startX;
    public  float driftDistance = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.position.x;

        // Auto-find SpriteRenderer if not assigned
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

        rb.velocity = new Vector2(driftDir * driftSpeed, 0f);
        FlipSprite(driftDir);
    }

    private void DoChase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dir * chaseSpeed, 0f);
        FlipSprite((int)dir);
    }

    // Flips the sprite so the enemy always faces its movement direction.
    // Assumes the sprite's default face direction is right (positive X).
    private void FlipSprite(int direction)
    {
        if (spriteRenderer == null) return;
        if (direction == 0) return;
        spriteRenderer.flipX = direction < 0;
    }
}