using UnityEngine;

public class WorldHealthDisplay : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.4f, 0f);

    [Header("Display")]
    [SerializeField] private TextMesh hpText;
    [SerializeField] private Transform fillBar;
    [SerializeField] private float fullBarWidth = 1.2f;

    public static WorldHealthDisplay Create(Transform target, EnemyHealth enemyHealth, BossHealth bossHealth, Vector3 offset, float width)
    {
        GameObject display = new GameObject("HP Display");
        display.transform.SetParent(target, false);
        display.transform.localPosition = offset;

        WorldHealthDisplay healthDisplay = display.AddComponent<WorldHealthDisplay>();
        healthDisplay.target = target;
        healthDisplay.enemyHealth = enemyHealth;
        healthDisplay.bossHealth = bossHealth;
        healthDisplay.worldOffset = offset;
        healthDisplay.fullBarWidth = width;

        GameObject textObject = new GameObject("HP Text");
        textObject.transform.SetParent(display.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.text = "HP";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.16f;
        text.fontSize = 48;
        text.color = Color.white;
        healthDisplay.hpText = text;

        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.name = "HP Bar Background";
        background.transform.SetParent(display.transform, false);
        background.transform.localPosition = new Vector3(0f, -0.18f, 0.01f);
        background.transform.localScale = new Vector3(width, 0.1f, 0.08f);
        background.GetComponent<Renderer>().material.color = new Color(0.02f, 0.02f, 0.02f);
        Collider backgroundCollider = background.GetComponent<Collider>();
        if (backgroundCollider != null)
        {
            Destroy(backgroundCollider);
        }

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HP Bar Fill";
        fill.transform.SetParent(display.transform, false);
        fill.transform.localPosition = new Vector3(0f, -0.18f, 0f);
        fill.transform.localScale = new Vector3(width, 0.08f, 0.08f);
        fill.GetComponent<Renderer>().material.color = new Color(0.05f, 0.9f, 0.25f);
        Collider fillCollider = fill.GetComponent<Collider>();
        if (fillCollider != null)
        {
            Destroy(fillCollider);
        }

        healthDisplay.fillBar = fill.transform;
        return healthDisplay;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + worldOffset;

        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        float maxHealth = GetMaxHealth();
        float currentHealth = Mathf.Clamp(GetCurrentHealth(), 0f, maxHealth);
        float percent = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        if (hpText != null)
        {
            hpText.text = "HP " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth);
        }

        if (fillBar != null)
        {
            fillBar.localScale = new Vector3(fullBarWidth * percent, 0.08f, 0.08f);
            fillBar.localPosition = new Vector3((percent - 1f) * fullBarWidth * 0.5f, -0.18f, 0f);
        }
    }

    private float GetCurrentHealth()
    {
        if (enemyHealth != null) return enemyHealth.CurrentHealth;
        if (bossHealth != null) return bossHealth.CurrentHealth;
        return 0f;
    }

    private float GetMaxHealth()
    {
        if (enemyHealth != null) return enemyHealth.MaxHealth;
        if (bossHealth != null) return bossHealth.MaxHealth;
        return 1f;
    }
}
