using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab ศัตรู
    public Transform spawnPoint; // จุดเกิดศัตรู
    public float spawnRate = 3f; // เวลาสร้างศัตรูใหม่
    public int maxEnemies = 5; // จำนวนศัตรูสูงสุดในฉาก

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
        // Raycast ลงด้านล่างเพื่อหาพื้นที่ว่างที่ไม่ใช่กำแพง
        Vector3 start = spawnPoint.position + Vector3.up * 2; // เริ่ม Raycast สูงขึ้น
        RaycastHit hit;
        if (Physics.Raycast(start, Vector3.down, out hit, 10f))
        {
            return hit.point; // ใช้จุดที่ Raycast โดน
        }
        return spawnPoint.position; // ถ้าไม่เจอ อิงตำแหน่งเดิม
    }

    public void EnemyDied()
    {
        currentEnemyCount--;
    }
}
