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
        Agent agent = GameObject.FindWithTag("player")?.GetComponent<Agent>();
        if (agent != null)
        {
            agent.AddReward(+1f);
        }

        if (KillCoutManager.Instance != null)
        {
            KillCoutManager.Instance.AddCoin(1);
            //agent?.AddReward(+0.3f); 
        }

        if (waveManager != null)
        {
            //agent?.AddReward(+0.2f); 
            waveManager.OnEnemyKilled();
        }

        Destroy(gameObject);
    }
}
