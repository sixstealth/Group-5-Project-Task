using UnityEngine;

public class PlayerHiding : MonoBehaviour
{
    [Header("State")]
    public bool isHiding;

    [Header("Detection")]
    [SerializeField] private string hidingSpotTag = "HidingSpot";

    private int hidingOverlapCount;
    private PlayerVisibility playerVisibility;

    private void Awake()
    {
        playerVisibility = GetComponent<PlayerVisibility>();
        SetHiding(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHidingSpot(other)) return;

        hidingOverlapCount++;
        SetHiding(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsHidingSpot(other)) return;

        hidingOverlapCount = Mathf.Max(0, hidingOverlapCount - 1);
        if (hidingOverlapCount == 0)
        {
            SetHiding(false);
        }
    }

    private bool IsHidingSpot(Collider other)
    {
        return other.tag == hidingSpotTag || other.GetComponent<HidingSpot>() != null;
    }

    private void SetHiding(bool value)
    {
        if (isHiding == value) return;

        isHiding = value;

        if (playerVisibility != null)
        {
            playerVisibility.SetHidden(isHiding);
        }

        if (isHiding)
        {
            CorridorEnemyAI[] enemies = FindObjectsOfType<CorridorEnemyAI>();
            foreach (CorridorEnemyAI enemy in enemies)
            {
                enemy.OnPlayerHidden(transform);
            }
        }
    }
}
