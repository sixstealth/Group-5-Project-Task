using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 75f;

    private float currentHealth;
    private bool isDead;
    private CorridorEnemyAI enemyAI;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<CorridorEnemyAI>();

        if (GetComponentInChildren<WorldHealthDisplay>() == null)
        {
            WorldHealthDisplay.Create(transform, this, null, new Vector3(0f, 2.25f, 0f), 1.2f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (enemyAI != null)
        {
            enemyAI.Die();
            return;
        }

        Destroy(gameObject);
    }
}
