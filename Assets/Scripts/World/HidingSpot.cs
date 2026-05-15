using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HidingSpot : MonoBehaviour
{
    private void Reset()
    {
        Collider spotCollider = GetComponent<Collider>();
        spotCollider.isTrigger = true;
    }
}
