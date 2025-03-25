using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // ���Ὼ�ѵ�� / Enemy prefab
    public Transform spawnPoint;        // ���˹��Դ�ѵ�� / Spawn point
    public float spawnRate = 3f;        // �������ҧ�ѵ������ / Spawn interval
    public int maxEnemies = 5;          // �ӹǹ�ѵ���٧�ش / Max enemies in scene

    private int currentEnemyCount = 0;  // �Ѻ�ӹǹ�ѵ�ٻѨ�غѹ / Current enemy count

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate); // ���ҧ��ӵ������ / Repeated spawn
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount < maxEnemies)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(); // �ҵ��˹觷���ҧ�ѵ����
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        // Raycast ŧ��ҹ��ҧ�ҡ���˹� spawn ������ "���"
        Vector3 start = spawnPoint.position + Vector3.up * 2f;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 10f))
        {
            return hit.point; // ����;�� �׹���˹觾��
        }

        return spawnPoint.position; // �������;�� ����˹����
    }

    public void EnemyDied()
    {
        currentEnemyCount--; // Ŵ�ӹǹ������ѵ�ٵ�� / Decrease count when enemy dies
    }

    /// <summary>
    /// ź�ѵ�ٷ����� ������絨ӹǹ / Destroy all enemies and reset count
    /// </summary>
    public void ResetEnemies()
    {
        // ���ѵ�ٷ���������� tag = "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        currentEnemyCount = 0;

        // ���ҧ����á�ѹ�� (��ҵ�ͧ���) / Optionally spawn one now
        SpawnEnemy();
    }
}
