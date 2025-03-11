using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab �ѵ��
    public Transform spawnPoint; // �ش�Դ�ѵ��
    public float spawnRate = 3f; // �������ҧ�ѵ������
    public int maxEnemies = 5; // �ӹǹ�ѵ���٧�ش㹩ҡ

    private int currentEnemyCount = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate);
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount < maxEnemies)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        // Raycast ŧ��ҹ��ҧ�����Ҿ�鹷����ҧ���������ᾧ
        Vector3 start = spawnPoint.position + Vector3.up * 2; // ����� Raycast �٧���
        RaycastHit hit;
        if (Physics.Raycast(start, Vector3.down, out hit, 10f))
        {
            return hit.point; // ��ش��� Raycast ⴹ
        }
        return spawnPoint.position; // �������� �ԧ���˹����
    }

    public void EnemyDied()
    {
        currentEnemyCount--;
    }
}
