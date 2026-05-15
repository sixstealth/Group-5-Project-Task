using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 300f;

    [Header("Death")]
    [SerializeField] private Level3Manager level3Manager;
    [SerializeField] private bool disableOnDeath = true;

    private float currentHealth;
    private bool isDead;

    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (GetComponentInChildren<WorldHealthDisplay>() == null)
        {
            WorldHealthDisplay.Create(transform, null, this, new Vector3(0f, 3.6f, 0f), 1.8f);
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

        if (level3Manager == null)
        {
            level3Manager = FindObjectOfType<Level3Manager>();
        }

        if (level3Manager != null)
        {
            level3Manager.OnBossDefeated();
        }

        if (disableOnDeath)
        {
            gameObject.SetActive(false);
        }
    }
}
