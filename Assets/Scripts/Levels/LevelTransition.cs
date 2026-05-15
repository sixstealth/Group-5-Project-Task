using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string nextSceneName = "Level3";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == playerTag)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
