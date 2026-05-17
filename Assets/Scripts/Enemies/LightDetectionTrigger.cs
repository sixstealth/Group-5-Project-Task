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
    [SerializeField] private float maxVerticalDetectionDifference = 2f;

    [Header("Light Feedback")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float baseIntensity = 1000f;
    [SerializeField] private float detectedIntensity = 5000f;
    [SerializeField] private float flashFadeSpeed = 4f;

    private Transform detectedPlayer;
    private Collider detectedPlayerCollider;
    private PlayerHiding detectedPlayerHiding;
    private float detectionTimer;
    private float alertTimer;
    private float currentIntensity;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (associatedEnemy == null)
        {
            associatedEnemy = GetComponentInParent<CorridorEnemyAI>();
        }

        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }
    }

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

        if (!IsInsideDetectionBeam(detectedPlayerCollider))
        {
            ClearDetection();
            return;
        }

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
        if (!IsInsideDetectionBeam(other)) return;

        StartDetection(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (detectedPlayer != null || !IsPlayer(other)) return;
        if (!IsInsideDetectionBeam(other)) return;

        StartDetection(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        ClearDetection();
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag(playerTag);
    }

    private void StartDetection(Collider other)
    {
        detectedPlayer = other.transform;
        detectedPlayerCollider = other;
        detectedPlayerHiding = other.GetComponentInParent<PlayerHiding>();
        detectionTimer = detectionDelay;
        alertTimer = 0f;

        FlashLight();

        if (!IsPlayerHidden() && detectionDelay <= 0f)
        {
            AlertEnemies(detectedPlayer);
        }
    }

    private void ClearDetection()
    {
        detectedPlayer = null;
        detectedPlayerCollider = null;
        detectedPlayerHiding = null;
        detectionTimer = detectionDelay;
    }

    private bool IsInsideDetectionBeam(Collider playerCollider)
    {
        if (playerCollider == null) return false;

        Vector3 localPoint = transform.InverseTransformPoint(playerCollider.bounds.center);
        if (localPoint.z < 0f) return false;

        float beamRange = spotLight != null ? spotLight.range : 8f;
        if (localPoint.z > beamRange) return false;

        if (Mathf.Abs(localPoint.y) > maxVerticalDetectionDifference) return false;

        float angle = spotLight != null ? spotLight.spotAngle : 35f;
        float radiusAtPoint = Mathf.Tan(angle * Mathf.Deg2Rad * 0.5f) * localPoint.z;
        return Mathf.Abs(localPoint.x) <= radiusAtPoint && Mathf.Abs(localPoint.y) <= radiusAtPoint;
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
