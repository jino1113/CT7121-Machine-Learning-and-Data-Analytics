using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    private EnemySpawner spawner;

    void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>(); // ���Ը�����᷹
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (spawner != null)
        {
            spawner.EnemyDied(); // �� Spawner ����ѵ�ٵ�ǹ��������
        }
        Destroy(gameObject);
    }
}
