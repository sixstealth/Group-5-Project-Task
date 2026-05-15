using UnityEngine;
using UnityEngine.UI;

public class Level1Manager : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] private int requiredStars = 4;
    [SerializeField] private Text starCounterText;

    [Header("Key")]
    [SerializeField] private GameObject keyObject;
    [SerializeField] private bool hideKeyOnStart = true;

    private int collectedStars;
    private bool hasKey;

    public int CollectedStars => collectedStars;
    public int RequiredStars => requiredStars;
    public bool HasKey => hasKey;

    private void Start()
    {
        if (keyObject != null && hideKeyOnStart)
        {
            keyObject.SetActive(false);
        }

        UpdateUI();
    }

    public void CollectStar()
    {
        collectedStars++;
        UpdateUI();

        if (collectedStars >= requiredStars && keyObject != null)
        {
            keyObject.SetActive(true);
        }
    }

    public void CollectKey()
    {
        hasKey = true;
    }

    private void UpdateUI()
    {
        if (starCounterText != null)
        {
            starCounterText.text = "Stars: " + collectedStars + " / " + requiredStars;
        }
    }
}
