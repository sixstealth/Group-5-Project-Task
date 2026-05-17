using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3Manager : MonoBehaviour
{
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private string endingSceneName = "EndingCutscene";
    [SerializeField] private float endingDelay = 4.5f;
    [SerializeField] private GameObject[] lockedObjectsUntilBossDies;

    private bool bossDefeated;

    private void Start()
    {
        if (bossHealth == null)
        {
            bossHealth = FindObjectOfType<BossHealth>();
        }
    }

    public void OnBossDefeated()
    {
        if (bossDefeated) return;

        bossDefeated = true;

        foreach (GameObject lockedObject in lockedObjectsUntilBossDies)
        {
            if (lockedObject != null)
            {
                lockedObject.SetActive(false);
            }
        }

        Invoke(nameof(LoadEnding), endingDelay);
    }

    private void LoadEnding()
    {
        SceneManager.LoadScene(endingSceneName);
    }
}
