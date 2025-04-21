using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f;
    public int maxBounces = 3;

    private int bounceCount = 0;
    private Rigidbody rb;

    [HideInInspector] public PlayerAgent agent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifeTime); // Auto destroy after time
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (bounceCount < maxBounces)
            {
                Vector3 reflectDir = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
                rb.velocity = reflectDir * speed;
                bounceCount++;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            DealDamage(collision.gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            DealDamage(other.gameObject);
            Destroy(gameObject);
        }
    }

    void DealDamage(GameObject target)
    {
        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);

            if (agent != null)
            {
                agent.AddReward(1f);
            }
        }
    }
}
