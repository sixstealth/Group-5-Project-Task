using UnityEngine;

public class FlashlightCone : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";
    public float  broadcastRadius        = 12f;
    public float  continuousAlertInterval = 1.5f;

    [Header("Light Feedback (optional)")]
    public Light spotLight;
    public float baseIntensity     = 1.0f;
    public float detectedIntensity = 2.2f;
    public float flashFadeSpeed    = 4f;

    private Transform _detectedPlayer  = null;
    private float     _continuousTimer = 0f;
    private float     _currentIntensity;

    private void Start()
    {
        _currentIntensity = baseIntensity;
        if (spotLight != null) spotLight.intensity = baseIntensity;
    }

    private void Update()
    {
        if (spotLight != null && _currentIntensity > baseIntensity)
        {
            _currentIntensity   = Mathf.MoveTowards(_currentIntensity, baseIntensity, flashFadeSpeed * Time.deltaTime);
            spotLight.intensity = _currentIntensity;
        }

        if (_detectedPlayer != null)
        {
            _continuousTimer -= Time.deltaTime;
            if (_continuousTimer <= 0f)
            {
                _continuousTimer = continuousAlertInterval;
                BroadcastAlert(_detectedPlayer);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _detectedPlayer  = other.transform;
        _continuousTimer = continuousAlertInterval;

        _currentIntensity = detectedIntensity;
        if (spotLight != null) spotLight.intensity = detectedIntensity;

        BroadcastAlert(_detectedPlayer);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _detectedPlayer = null;
    }

    private void BroadcastAlert(Transform detectedPlayer)
    {
        if (detectedPlayer == null) return;

        foreach (var enemy in FindObjectsOfType<CorridorEnemyAI>())
        {
            if (enemy == null) continue;
            if (Vector3.Distance(transform.position, enemy.transform.position) > broadcastRadius) continue;
            enemy.OnFlashlightDetected(detectedPlayer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, broadcastRadius);
    }
}