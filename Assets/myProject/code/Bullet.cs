using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f; // เวลาก่อนที่กระสุนจะหายไป
    public int maxBounces = 3; // จำนวนครั้งที่เด้งได้
    private int bounceCount = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed; // ให้กระสุนเคลื่อนที่ทันที
        Destroy(gameObject, lifeTime); // ลบกระสุนเมื่อถึงเวลา
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // ถ้ากระสุนโดนศัตรู
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // ทำดาเมจให้ศัตรู
            }
            Destroy(gameObject); // ทำลายกระสุนเมื่อโดนศัตรู
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) // ถ้ากระสุนชนกำแพง
        {
            if (bounceCount < maxBounces) // ถ้ายังไม่เกินจำนวนครั้งที่เด้งได้
            {
                Vector3 reflectDir = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal); // คำนวณทิศทางเด้ง
                rb.velocity = reflectDir * speed; // อัปเดตความเร็วไปตามทิศทางใหม่
                bounceCount++;
            }
            else
            {
                Destroy(gameObject); // ถ้าเด้งครบแล้ว ให้ทำลายกระสุน
            }
        }
    }
}
