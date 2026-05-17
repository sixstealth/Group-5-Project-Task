using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 300f;

    [Header("Death")]
    [SerializeField] private Level3Manager level3Manager;
    [SerializeField] private bool disableOnDeath;
    [SerializeField] private float deathAnimationDelay = 4.5f;

    private float currentHealth;
    private bool isDead;
    private BossController bossController;

    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        bossController = GetComponent<BossController>();
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

        if (bossController != null)
        {
            bossController.PlayDeathAnimation();
        }

        if (level3Manager != null)
        {
            level3Manager.OnBossDefeated();
        }

        if (disableOnDeath)
        {
            Invoke(nameof(DisableBoss), Mathf.Max(0f, deathAnimationDelay));
        }
    }

    private void DisableBoss()
    {
        gameObject.SetActive(false);
    }
}
