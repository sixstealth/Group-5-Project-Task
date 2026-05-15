using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Level1";
    [SerializeField] private bool autoAdvance = true;
    [SerializeField] private float autoAdvanceSeconds = 5f;
    [SerializeField] private Button continueButton;

    private float timer;

    private void Start()
    {
        timer = autoAdvanceSeconds;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(Continue);
        }
    }

    private void Update()
    {
        if (!autoAdvance) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Continue();
        }
    }

    public void Continue()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
