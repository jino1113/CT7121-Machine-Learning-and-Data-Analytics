using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 5f; // ���ҡ�͹������ع������
    public int maxBounces = 3; // �ӹǹ���駷������
    private int bounceCount = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed; // ������ع����͹���ѹ��
        Destroy(gameObject, lifeTime); // ź����ع����Ͷ֧����
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // ��ҡ���عⴹ�ѵ��
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // �Ӵ��������ѵ��
            }
            Destroy(gameObject); // ����¡���ع�����ⴹ�ѵ��
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) // ��ҡ���ع����ᾧ
        {
            if (bounceCount < maxBounces) // ����ѧ����Թ�ӹǹ���駷������
            {
                Vector3 reflectDir = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal); // �ӹǳ��ȷҧ��
                rb.velocity = reflectDir * speed; // �ѻവ��������仵����ȷҧ����
                bounceCount++;
            }
            else
            {
                Destroy(gameObject); // ����駤ú���� ������¡���ع
            }
        }
    }
}
