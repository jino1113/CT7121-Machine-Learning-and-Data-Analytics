using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public float attackRange = 1.5f;
    public int damage = 1;

    private bool canAttack = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);

            // ตรวจสอบระยะโจมตี
            if (Vector3.Distance(transform.position, player.position) <= attackRange && canAttack)
            {
                StartCoroutine(AttackPlayer());
            }
        }
    }

    IEnumerator AttackPlayer()
    {
        canAttack = false;
        player.GetComponent<PlayerHealth>().TakeDamage(damage);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject); // ศัตรูหายไปเมื่อกัดผู้เล่น
    }
}
