using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private EnemySpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>(); // ใช้วิธีใหม่แทน
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (spawner != null)
        {
            spawner.EnemyDied(); // แจ้ง Spawner ว่าศัตรูตัวนี้ตายแล้ว
        }
        Destroy(gameObject);
    }
}
