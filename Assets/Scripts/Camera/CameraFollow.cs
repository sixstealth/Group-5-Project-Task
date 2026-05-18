using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Follow Settings")]
    [SerializeField] private float distance = 7f;
    [SerializeField] private float targetHeight = 1.2f;
    [SerializeField] private float positionSmoothTime = 0.12f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = 20f;
    [SerializeField] private float maxPitch = 75f;

    [Header("Start Camera Angle")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 45f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorWhileRotating = false;

    private Vector3 currentVelocity;

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
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleMouseRotation();
        FollowTarget();
    }

    private void HandleMouseRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (lockCursorWhileRotating)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            if (lockCursorWhileRotating)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void FollowTarget()
    {
        Vector3 targetPoint = target.position + Vector3.up * targetHeight;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = targetPoint - rotation * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.LookAt(targetPoint);
    }
}