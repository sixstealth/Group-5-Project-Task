using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleStar : MonoBehaviour
{
    [SerializeField] private Level1Manager levelManager;
    [SerializeField] private string playerTag = "Player";

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != playerTag) return;

        if (levelManager != null)
        {
            levelManager.CollectStar();
        }

        Destroy(gameObject);
    }
}
