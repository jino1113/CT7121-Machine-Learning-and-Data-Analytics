using Unity.MLAgents;
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
        // ตรวจสอบว่าเจอ GameObject ที่มี tag "player" และมี Agent หรือไม่ / Check if we can find the Agent from the player tag
        Agent agent = GameObject.FindWithTag("player")?.GetComponent<Agent>();
        if (agent != null)
        {
            agent.AddReward(+1f); // เพิ่มรางวัลเมื่อฆ่าศัตรู / Give reward when enemy is killed
        }

        // แจ้ง EnemySpawner ว่าศัตรูถูกกำจัดแล้ว / Notify the spawner that the enemy is dead
        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        // ทำลายวัตถุของศัตรู / Destroy enemy object
        Destroy(gameObject);
    }
}
