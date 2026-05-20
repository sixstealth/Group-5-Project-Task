using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkThreshold = 0.05f;
    [SerializeField] private float runSpeedValue = 1f;
    [SerializeField] private float walkSpeedValue = 0.5f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Input")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

    private Vector3 lastPosition;
    private float currentAnimSpeed;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (animator == null) return;

        UpdateMovementAnimation();
        UpdateActionAnimation();

        lastPosition = transform.position;
    }

    private void UpdateMovementAnimation()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;

        float movementAmount = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        float targetSpeed = 0f;

        if (movementAmount > walkThreshold)
        {
            bool isRunning = Input.GetKey(runKey);
            targetSpeed = isRunning ? runSpeedValue : walkSpeedValue;
        }

        currentAnimSpeed = Mathf.Lerp(
            currentAnimSpeed,
            targetSpeed,
            Time.deltaTime * smoothSpeed
        );

        animator.SetFloat("Speed", currentAnimSpeed);
    }

    private void UpdateActionAnimation()
    {
        if (Input.GetKeyDown(attackKey))
        {
            animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(jumpKey))
        {
            animator.SetTrigger("Jump");
        }
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger("Attack");
    }

    public void PlayJump()
    {
        if (animator == null) return;
        animator.SetTrigger("Jump");
    }

    public void SetGrounded(bool grounded)
    {
        if (animator == null) return;
        animator.SetBool("Grounded", grounded);
    }
}