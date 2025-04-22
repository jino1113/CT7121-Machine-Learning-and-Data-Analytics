using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollectibleSpawner : MonoBehaviour
{
    public CollectibleData[] collectibles; 
    public Transform[] spawnPoints; 
    public int spawnCount = 3;

    private int currentCollectibles = 0;
    public int maxActiveCollectibles = 10;

    void Start()
    {
        HideSpawnPoints();

        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        for (int i = 0; i < spawnCount && availablePoints.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform point = availablePoints[randomIndex];

            SpawnAtPoint(point);
            availablePoints.RemoveAt(randomIndex); // เอาจุดนั้นออกจาก list
        }
    }

    void HideSpawnPoints()
    {
        foreach (Transform point in spawnPoints)
        {
            point.gameObject.SetActive(false); 
        }
    }

    void SpawnAtPoint(Transform point)
    {
        if (currentCollectibles >= maxActiveCollectibles) return;

        Collider[] hits = Physics.OverlapSphere(point.position, 0.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Collectible"))
            {
                return; 
            }
        }

        var data = collectibles[Random.Range(0, collectibles.Length)];
        GameObject go = Instantiate(data.modelPrefab, point.position, point.rotation);

        var col = go.GetComponent<Collectible>();
        if (col != null)
        {
            col.spawner = this;
            col.spawnedFrom = point;
            col.onCollected += HandleCollectibleCollected;
        }

        currentCollectibles++;
    }


    void HandleCollectibleCollected()
    {
        currentCollectibles--;
    }

    public void RespawnCollectible(Transform spawnPoint, float delay)
    {
        StartCoroutine(RespawnCoroutine(spawnPoint, delay));
    }

    private IEnumerator RespawnCoroutine(Transform point, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnAtPoint(point);
    }
}
