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

        // ค้นหา player ถ้ายังไม่ได้เซ็ตไว้ใน Inspector
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("ไม่พบ GameObject ที่มี Tag = player");
            }
        }
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

        if (player != null)
        {
            PlayerHealth ph = player.root.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            else
            {
                Debug.LogError("Player have no PlayerHealth");
            }
        }

        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject); // ศัตรูหายไปเมื่อกัดผู้เล่น
    }

}
