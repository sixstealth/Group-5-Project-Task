using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class DoorUnlock : MonoBehaviour
{
    [SerializeField] private Level1Manager levelManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string nextSceneName = "Level2";
    [SerializeField] private Text lockedMessageText;
    [SerializeField] private string lockedMessage = "You need a key";
    [SerializeField] private float messageTime = 2f;

    private float messageTimer;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (levelManager == null)
        {
            levelManager = FindObjectOfType<Level1Manager>();
        }

        HideMessage();
    }

    private void Update()
    {
        if (lockedMessageText == null || messageTimer <= 0f) return;

        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f)
        {
            HideMessage();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != playerTag) return;

        if (levelManager != null && levelManager.HasKey)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        ShowMessage();
    }

    private void ShowMessage()
    {
        if (lockedMessageText == null) return;

        lockedMessageText.text = lockedMessage;
        lockedMessageText.gameObject.SetActive(true);
        messageTimer = messageTime;
    }

    private void HideMessage()
    {
        if (lockedMessageText != null)
        {
            lockedMessageText.gameObject.SetActive(false);
        }
    }
}
