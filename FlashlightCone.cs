public class FlashlightCone : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag on the player GameObject")]
    public string playerTag = "Player";
 
    [Tooltip("Radius from this flashlight within which Soot-Sprites will be alerted")]
    public float broadcastRadius = 12f;
 
    [Tooltip("Re-broadcast alert every N seconds while the player stays in the beam")]
    public float continuousAlertInterval = 1.5f;
 
    [Header("Light feedback (URP Light2D — optional)")]
    [Tooltip("The spotlight Light2D on this object, for a brief intensity flash on detection")]
    public Light2D spotLight;
    public float baseIntensity   = 1.0f;
    public float detectedIntensity = 2.2f;
    [Tooltip("How quickly the flash fades back to baseIntensity")]
    public float flashFadeSpeed  = 4f;
 
    private Transform _detectedPlayer     = null;
    private float     _continuousTimer    = 0f;
    private float     _currentIntensity;
 
    private void Start()
    {
        _currentIntensity = baseIntensity;
        if (spotLight != null) spotLight.intensity = baseIntensity;
    }
 
    private void Update()
    {
        // Fade the intensity flash back to normal
        if (spotLight != null && _currentIntensity > baseIntensity)
        {
            _currentIntensity  = Mathf.MoveTowards(_currentIntensity, baseIntensity, flashFadeSpeed * Time.deltaTime);
            spotLight.intensity = _currentIntensity;
        }
 
        // Continuous alert while player stays in beam
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
 
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
 
        _detectedPlayer  = other.transform;
        _continuousTimer = continuousAlertInterval;
 
        // Flash the light on initial detection
        _currentIntensity = detectedIntensity;
        if (spotLight != null) spotLight.intensity = detectedIntensity;
 
        BroadcastAlert(_detectedPlayer);
    }
 
    private void OnTriggerExit2D(Collider2D other)
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
 
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist > broadcastRadius) continue;
 
            enemy.OnFlashlightDetected(detectedPlayer);
        }
    }
 
    private void OnDrawGizmosSelected()
    {
        // Cyan: how far the flashlight alert broadcasts to Soot-Sprites
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, broadcastRadius);
    }
}