using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private float damageMultiplier = 2f;

    private void Reset()
    {
        bossHealth = GetComponentInParent<BossHealth>();
    }

    private void Awake()
    {
        if (bossHealth == null)
        {
            bossHealth = GetComponentInParent<BossHealth>();
        }
    }

    public void RegisterHit(float baseDamage)
    {
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(baseDamage * damageMultiplier);
        }
    }
}
