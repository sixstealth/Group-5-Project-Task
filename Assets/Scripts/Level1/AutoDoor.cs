using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Door Parts")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Transform hingePoint;

    [Header("Door Blocking Collider")]
    [SerializeField] private Collider doorBlockingCollider;
    [SerializeField] private bool disableColliderWhenOpen = true;

    [Header("Opening")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private float closeDelay = 0.3f;

    private readonly HashSet<Collider> playerCollidersInside = new HashSet<Collider>();

    private Coroutine currentCoroutine;
    private float currentAngle;
    private bool isOpen;

    private void Start()
    {
        if (doorBlockingCollider != null)
        {
            doorBlockingCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerCollidersInside.Add(other);
        OpenDoor();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerCollidersInside.Remove(other);

        if (playerCollidersInside.Count == 0)
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        if (doorTransform == null || hingePoint == null) return;
        if (isOpen) return;

        isOpen = true;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        yield return RotateDoorTo(openAngle);

        if (disableColliderWhenOpen && doorBlockingCollider != null)
        {
            doorBlockingCollider.enabled = false;
        }
    }

    private void CloseDoor()
    {
        if (doorTransform == null || hingePoint == null) return;
        if (!isOpen) return;

        isOpen = false;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        if (playerCollidersInside.Count > 0)
        {
            yield break;
        }

        if (doorBlockingCollider != null)
        {
            doorBlockingCollider.enabled = true;
        }

        yield return RotateDoorTo(0f);
    }

    private IEnumerator RotateDoorTo(float targetAngle)
    {
        float startAngle = currentAngle;
        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / openDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            float newAngle = Mathf.Lerp(startAngle, targetAngle, t);
            float deltaAngle = newAngle - currentAngle;

            doorTransform.RotateAround(
                hingePoint.position,
                rotationAxis.normalized,
                deltaAngle
            );

            currentAngle = newAngle;

            yield return null;
        }

        float finalDelta = targetAngle - currentAngle;

        doorTransform.RotateAround(
            hingePoint.position,
            rotationAxis.normalized,
            finalDelta
        );

        currentAngle = targetAngle;
    }
}