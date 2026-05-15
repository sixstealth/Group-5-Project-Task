using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private bool useCameraDirection = false;

    private void Start()
    {
        if (attackOrigin == null && useCameraDirection && Camera.main != null)
        {
            attackOrigin = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Vector3 direction = useCameraDirection && origin != transform ? origin.forward : transform.forward;

        if (!Physics.Raycast(origin.position, direction, out RaycastHit hit, attackRange, hitLayers, QueryTriggerInteraction.Collide))
        {
            return;
        }

        BossWeakPoint weakPoint = hit.collider.GetComponentInParent<BossWeakPoint>();
        if (weakPoint != null)
        {
            weakPoint.RegisterHit(attackDamage);
            return;
        }

        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
            return;
        }

        BossHealth bossHealth = hit.collider.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Vector3 direction = useCameraDirection && origin != transform ? origin.forward : transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin.position, direction * attackRange);
    }
}
