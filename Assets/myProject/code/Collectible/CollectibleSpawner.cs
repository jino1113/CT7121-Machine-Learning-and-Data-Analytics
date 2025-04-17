using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    public CollectibleData[] collectibles; 
    public Transform[] spawnPoints; 
    public int spawnCount = 3;

    void Start()
    {
        HideSpawnPoints();

        for (int i = 0; i < spawnCount && i < spawnPoints.Length; i++)
        {
            SpawnAtPoint(spawnPoints[i]);
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
        var data = collectibles[Random.Range(0, collectibles.Length)];
        Instantiate(data.modelPrefab, point.position, point.rotation);
    }
}
