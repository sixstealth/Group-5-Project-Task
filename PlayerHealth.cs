using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;

    public void TakeDamage(int damage, Vector2 sourcePosition, float knockback)
    {
        health -= damage;
        Debug.Log("Player was hit! HP left:" + health);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 pushDir = ((Vector2)transform.position - sourcePosition).normalized;
            rb.AddForce(pushDir * knockback, ForceMode2D.Impulse);
        }
    }
}