using Unity.MLAgents;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleData data;

    [Header("Visual Effects Settings")]
    public float rotationSpeed = 90f;         
    public float floatAmplitude = 0.25f;      
    public float floatFrequency = 1f;         
    public float flashFrequency = 2f;
    public float Respawn;

    private Vector3 startPos;
    private Renderer rend;
    private Color originalColor;

    public CollectibleSpawner spawner;
    public Transform spawnedFrom;

    public System.Action onCollected;

    private void Start()
    {
        startPos = transform.position;
        rend = GetComponent<Renderer>();

        if (rend != null)
            originalColor = rend.material.color;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (rend != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashFrequency));
            Color c = originalColor;
            c.a = alpha;
            rend.material.color = c;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            Agent agent = GameObject.FindWithTag("player")?.GetComponent<Agent>();
            CollectibleManager.Instance.AddCollectible(data.collectibleName);

            if (spawner != null && spawnedFrom != null)
                spawner.RespawnCollectible(spawnedFrom, Respawn);

            onCollected?.Invoke();

            Destroy(gameObject);
            agent.AddReward(+3f);
        }
    }
}
