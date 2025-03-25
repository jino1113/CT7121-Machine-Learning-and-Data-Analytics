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

        // ���� player ����ѧ����������� Inspector
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("��辺 GameObject ����� Tag = player");
            }
        }
    }


    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);

            // ��Ǩ�ͺ��������
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
        Destroy(gameObject); // �ѵ����������͡Ѵ������
    }

}
