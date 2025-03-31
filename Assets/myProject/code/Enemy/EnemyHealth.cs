using Unity.MLAgents;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private EnemySpawner spawner;
    [SerializeField] private WaveManager waveManager; // Add reference to WaveManager

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>(); // ใช้วิธีใหม่แทน
        waveManager = FindFirstObjectByType<WaveManager>(); // Find the WaveManager
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

        // Notify WaveManager
        if (waveManager != null)
        {
            agent.AddReward(+5f);
            waveManager.OnEnemyKilled();
        }

        if (CoinManager.Instance != null)
        {
            agent.AddReward(+1f); // เพิ่มรางวัลเมื่อฆ่าศัตรู / Give reward when enemy is killed
            CoinManager.Instance.AddCoin(1);
            Debug.Log("Coin Added"); // ตรวจว่าเรียกจริง
        }

        // ทำลายวัตถุของศัตรู / Destroy enemy object
        Destroy(gameObject);
    }
}
