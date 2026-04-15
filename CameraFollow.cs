using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform target; 

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 6f, -6f); 
    
    public float smoothSpeed = 5f; 

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target);
    }
}