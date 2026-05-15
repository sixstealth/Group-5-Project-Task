using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string introSceneName = "IntroCutscene";

    public void Play()
    {
        SceneManager.LoadScene(introSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
