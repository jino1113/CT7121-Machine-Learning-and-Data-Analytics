using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;             // ความเร็วกระสุน / Bullet speed
    public float lifeTime = 5f;           // เวลาชีวิตกระสุน / Bullet lifetime
    public int maxBounces = 3;            // จำนวนครั้งที่เด้งได้ / Max number of bounces

    private int bounceCount = 0;          // จำนวนครั้งที่เด้งแล้ว / Current bounce count
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed; // เคลื่อนที่ไปข้างหน้า / Move forward
        Destroy(gameObject, lifeTime);           // ทำลายเมื่อหมดเวลา / Auto destroy
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

        // ถ้ากระสุนชนกำแพง
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
        // ถ้าชนศัตรูแบบไม่ใช้ Trigger
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            DealDamage(collision.gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ถ้าโดนศัตรูที่เป็น Trigger
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
            enemy.TakeDamage(1); // ทำดาเมจ / Apply damage
        }
    }
}
