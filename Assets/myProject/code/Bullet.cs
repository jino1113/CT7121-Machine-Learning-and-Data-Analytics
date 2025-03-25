using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;             // �������ǡ���ع / Bullet speed
    public float lifeTime = 5f;           // ���Ҫ��Ե����ع / Bullet lifetime
    public int maxBounces = 3;            // �ӹǹ���駷������ / Max number of bounces

    private int bounceCount = 0;          // �ӹǹ���駷�������� / Current bounce count
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed; // ����͹���仢�ҧ˹�� / Move forward
        Destroy(gameObject, lifeTime);           // ����������������� / Auto destroy
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found on Bullet!");
                return;
            }
        }

        // ��ҡ���ع����ᾧ
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
        // ��Ҫ��ѵ��Ẻ����� Trigger
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            DealDamage(collision.gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ���ⴹ�ѵ�ٷ���� Trigger
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
            enemy.TakeDamage(1); // �Ӵ���� / Apply damage
        }
    }
}
