using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // พรีแฟบศัตรู / Enemy prefab
    public Transform spawnPoint;        // ตำแหน่งเกิดศัตรู / Spawn point
    public float spawnRate = 3f;        // เวลาสร้างศัตรูใหม่ / Spawn interval
    public int maxEnemies = 5;          // จำนวนศัตรูสูงสุด / Max enemies in scene

    private int currentEnemyCount = 0;  // นับจำนวนศัตรูปัจจุบัน / Current enemy count

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate); // สร้างซ้ำตามเวลา / Repeated spawn
    }

    void SpawnEnemy()
    {
        if (currentEnemyCount < maxEnemies)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(); // หาตำแหน่งที่วางศัตรูได้
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        // Raycast ลงด้านล่างจากตำแหน่ง spawn เพื่อหา "พื้น"
        Vector3 start = spawnPoint.position + Vector3.up * 2f;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 10f))
        {
            return hit.point; // ถ้าเจอพื้น คืนตำแหน่งพื้น
        }

        return spawnPoint.position; // ถ้าไม่เจอพื้น ใช้ตำแหน่งเดิม
    }

    public void EnemyDied()
    {
        currentEnemyCount--; // ลดจำนวนเมื่อศัตรูตาย / Decrease count when enemy dies
    }

    /// <summary>
    /// ลบศัตรูทั้งหมด และรีเซ็ตจำนวน / Destroy all enemies and reset count
    /// </summary>
    public void ResetEnemies()
    {
        // หาศัตรูทั้งหมดที่มี tag = "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        currentEnemyCount = 0;

        // สร้างตัวแรกทันที (ถ้าต้องการ) / Optionally spawn one now
        SpawnEnemy();
    }
}
