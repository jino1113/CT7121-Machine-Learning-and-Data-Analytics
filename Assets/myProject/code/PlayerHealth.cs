using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Player HP: " + health);

        if (health <= 0)
        {
            Debug.Log("Player Died!");
            Destroy(gameObject);
        }
    }
}
