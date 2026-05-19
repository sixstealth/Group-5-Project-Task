using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Follow Settings")]
    [SerializeField] private float distance = 7f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float targetHeight = 1.2f;
    [SerializeField] private float positionSmoothTime = 0.08f;

    [Header("Mouse Rotation")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = 20f;
    [SerializeField] private float maxPitch = 75f;
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 45f;
    [SerializeField] private bool lockCursorOnPlay = true;

    [Header("Mouse Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomSmoothSpeed = 8f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionOffset = 0.25f;

    private Vector3 currentVelocity;
    private float targetDistance;

    private void Start()
    {
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        targetDistance = distance;

        if (lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleMouseRotation();
        HandleMouseZoom();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && lockCursorOnPlay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        FollowTargetWithCollision();
    }

    private void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleMouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * zoomSmoothSpeed);
    }

    private void FollowTargetWithCollision()
    {
        Vector3 targetPoint = target.position + Vector3.up * targetHeight;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 directionFromTargetToCamera = -(rotation * Vector3.forward);
        Vector3 desiredPosition = targetPoint + directionFromTargetToCamera * distance;

        Vector3 finalPosition = desiredPosition;

        if (Physics.SphereCast(
                targetPoint,
                collisionRadius,
                directionFromTargetToCamera,
                out RaycastHit hit,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            float correctedDistance = Mathf.Max(hit.distance - collisionOffset, minDistance);
            finalPosition = targetPoint + directionFromTargetToCamera * correctedDistance;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.LookAt(targetPoint);
    }
}