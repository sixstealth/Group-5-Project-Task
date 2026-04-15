using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; 
        rb.freezeRotation = true;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0f, z).normalized;
    }

    private void FixedUpdate()
    {
        Vector3 targetVelocity = moveInput * moveSpeed;
        
        targetVelocity.y = rb.velocity.y; 
        
        rb.velocity = targetVelocity;
    }
}