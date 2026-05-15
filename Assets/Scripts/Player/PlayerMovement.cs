using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 12f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool wantsToJump;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0f, z).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            wantsToJump = true;
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = GetCameraRelativeDirection(moveInput);
        float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? runSpeed : walkSpeed;

        Vector3 velocity = moveDirection * speed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (wantsToJump)
{
    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
}   

        wantsToJump = false;
    }

    private Vector3 GetCameraRelativeDirection(Vector3 input)
    {
        if (cameraTransform == null || input.sqrMagnitude < 0.001f)
        {
            return input;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (forward * input.z + right * input.x).normalized;
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.05f, groundLayers, QueryTriggerInteraction.Ignore);
    }
}
