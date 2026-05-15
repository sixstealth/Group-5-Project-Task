using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startingHealth = 100f;

    [Header("Death")]
    [SerializeField] private bool restartLevelOnDeath = true;
    [SerializeField] private float restartDelay = 0.4f;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(startingHealth, 0f, maxHealth);
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage(int damage, Vector3 sourcePosition, float knockback)
    {
        TakeDamage((float)damage);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null || knockback <= 0f) return;

        Vector3 pushDirection = (transform.position - sourcePosition).normalized;
        pushDirection.y = 0.5f;
        pushDirection.Normalize();
        rb.AddForce(pushDirection * knockback, ForceMode.Impulse);
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (restartLevelOnDeath)
        {
            Invoke(nameof(RestartCurrentLevel), restartDelay);
        }
    }

    private void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
        }
    }
}
