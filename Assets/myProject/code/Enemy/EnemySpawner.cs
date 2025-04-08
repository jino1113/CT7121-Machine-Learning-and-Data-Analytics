using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;     // Enemy prefab
    public GameObject target;
    public Transform[] spawnPoint;       // Spawn position
    public float spawnRate = 3f;       // Spawn interval
    public int maxEnemies = 5;         // Max enemies in this area
    public float detectionRadius = 10f;// Radius to check for enemies
    public LayerMask enemyLayer;       // Enemy layer

    public float boxWidth = 10f;
    public float boxHeight = 2f;
    public float boxDepth = 10f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnRate);
    }

    void SpawnEnemy()
    {
        if (spawnPoint == null)
        {
            Debug.LogError($"[EnemySpawner] SpawnPoint not assigned on {gameObject.name}");
            return;
        }

        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        int nearbyCount = 0;

        foreach (Collider col in nearbyEnemies)
        {
            if (col.CompareTag("Enemy"))
            {
                nearbyCount++;
            }
        }

        if (nearbyCount >= maxEnemies) return;

        Vector3 spawnPosition = Vector3.zero; // Declare it outside with a default

        List<Vector3> validSpawns = GetValidSpawnPosition();
        if (validSpawns.Count > 0)
        {
            spawnPosition = validSpawns[UnityEngine.Random.Range(0, validSpawns.Count)];
        }

        // Instantiate using the selected spawnPosition
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Assign player
        enemy.GetComponent<EnemyAI>().player = target.transform;

        // Stop NavMeshAgent motion right after spawn
        if (enemy.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        {
            agent.ResetPath(); // Cancel movement
            agent.velocity = Vector3.zero; // Stop instantly
        }
    }


    public List<Vector3> GetValidSpawnPosition()
    {
        List<Vector3> validSpawns = new List<Vector3>();

        for (int i = 0; i < spawnPoint.Length; i++)
        {
            Vector3 start = spawnPoint[i].position + Vector3.up * 2f;

            // Cast downward to check for ground
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 10f))
            {
                validSpawns.Add(hit.point); // Valid ground position
            }
        }

        return validSpawns;
    }


    public void ResetEnemies()
    {
        Vector3 halfExtents = new Vector3(boxWidth / 2f, boxHeight / 2f, boxDepth / 2f); // Set this based on your box size
        Collider[] enemies = Physics.OverlapBox(transform.position, halfExtents, Quaternion.identity, enemyLayer);
        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Destroy(enemy.gameObject);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 halfExtents = new Vector3(boxWidth / 2f, boxHeight / 2f, boxDepth / 2f); // Set this based on your box size
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, halfExtents);
    }
#endif
}
