using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightDetectionTrigger : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private CorridorEnemyAI associatedEnemy;
    [SerializeField] private float detectionDelay = 0f;
    [SerializeField] private float broadcastRadius = 12f;
    [SerializeField] private float continuousAlertInterval = 1.5f;

    [Header("Light Feedback")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float baseIntensity = 1000f;
    [SerializeField] private float detectedIntensity = 5000f;
    [SerializeField] private float flashFadeSpeed = 4f;

    private Transform detectedPlayer;
    private PlayerHiding detectedPlayerHiding;
    private float detectionTimer;
    private float alertTimer;
    private float currentIntensity;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        associatedEnemy = GetComponentInParent<CorridorEnemyAI>();
        spotLight = GetComponentInChildren<Light>();
    }

    private void Start()
    {
        currentIntensity = baseIntensity;
        if (spotLight != null)
        {
            spotLight.intensity = baseIntensity;
        }
    }

    private void Update()
    {
        FadeLight();

        if (detectedPlayer == null) return;

        if (IsPlayerHidden())
        {
            detectionTimer = detectionDelay;
            return;
        }

        detectionTimer -= Time.deltaTime;
        alertTimer -= Time.deltaTime;

        if (detectionTimer <= 0f && alertTimer <= 0f)
        {
            alertTimer = continuousAlertInterval;
            AlertEnemies(detectedPlayer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        detectedPlayer = other.transform;
        detectedPlayerHiding = other.GetComponent<PlayerHiding>();
        detectionTimer = detectionDelay;
        alertTimer = 0f;

        FlashLight();

        if (!IsPlayerHidden() && detectionDelay <= 0f)
        {
            AlertEnemies(detectedPlayer);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        detectedPlayer = null;
        detectedPlayerHiding = null;
        detectionTimer = detectionDelay;
    }

    private bool IsPlayer(Collider other)
    {
        return other.tag == playerTag;
    }

    private bool IsPlayerHidden()
    {
        return detectedPlayerHiding != null && detectedPlayerHiding.isHiding;
    }

    private void AlertEnemies(Transform player)
    {
        if (player == null) return;

        FlashLight();

        if (associatedEnemy != null)
        {
            associatedEnemy.OnFlashlightDetected(player);
            return;
        }

        CorridorEnemyAI[] enemies = FindObjectsOfType<CorridorEnemyAI>();
        foreach (CorridorEnemyAI enemy in enemies)
        {
            if (enemy == null) continue;
            if (Vector3.Distance(transform.position, enemy.transform.position) > broadcastRadius) continue;

            enemy.OnFlashlightDetected(player);
        }
    }

    private void FlashLight()
    {
        currentIntensity = detectedIntensity;
        if (spotLight != null)
        {
            spotLight.intensity = detectedIntensity;
        }
    }

    private void FadeLight()
    {
        if (spotLight == null || currentIntensity <= baseIntensity) return;

        currentIntensity = Mathf.MoveTowards(currentIntensity, baseIntensity, flashFadeSpeed * Time.deltaTime);
        spotLight.intensity = currentIntensity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, broadcastRadius);
    }
}
