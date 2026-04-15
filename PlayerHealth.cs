using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int health = 5;

    public void TakeDamage(int damage, Vector3 sourcePosition, float knockback)
    {
        health -= damage;
        Debug.Log("Player was hit! HP left: " + health);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pushDir = (transform.position - sourcePosition).normalized;
            pushDir.y = 0.5f;
            pushDir.Normalize();
            rb.AddForce(pushDir * knockback, ForceMode.Impulse);
        }
    }
}