using System.Collections;
using UnityEngine;

public class OpenableChest : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    [Header("Lid")]
    [SerializeField] private Transform lidPivot;

    [Header("Closed State")]
    [SerializeField] private bool useCurrentLidPositionAsClosed = true;
    [SerializeField] private Vector3 closedLocalPosition;
    [SerializeField] private Vector3 closedRotation = Vector3.zero;

    [Header("Opened State")]
    [SerializeField] private Vector3 openedLocalPosition;
    [SerializeField] private Vector3 openedRotation = new Vector3(-75f, 0f, 0f);
    [SerializeField] private float openDuration = 0.6f;

    [Header("Reward")]
    [SerializeField] private GameObject starInside;

    private bool playerIsNear;
    private bool isOpened;
    private bool isOpening;

    private void Start()
    {
        if (lidPivot != null)
        {
            if (useCurrentLidPositionAsClosed)
            {
                closedLocalPosition = lidPivot.localPosition;
            }

            lidPivot.localPosition = closedLocalPosition;
            lidPivot.localRotation = Quaternion.Euler(closedRotation);
        }

        if (starInside != null)
        {
            starInside.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerIsNear) return;
        if (isOpened || isOpening) return;

        if (Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenChest());
        }
    }

    private IEnumerator OpenChest()
    {
        if (lidPivot == null)
        {
            Debug.LogWarning("Lid Pivot is not assigned.");
            yield break;
        }

        isOpening = true;

        Vector3 startPosition = lidPivot.localPosition;
        Vector3 targetPosition = openedLocalPosition;

        Quaternion startRotation = lidPivot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(openedRotation);

        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float t = timer / openDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            lidPivot.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            lidPivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        lidPivot.localPosition = targetPosition;
        lidPivot.localRotation = targetRotation;

        isOpened = true;
        isOpening = false;

        if (starInside != null)
        {
            starInside.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsNear = false;
        }
    }
}