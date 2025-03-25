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

        // ✅ Stop any initial movement
        agent.ResetPath();               // Cancel any planned path
        agent.velocity = Vector3.zero;   // Ensure it's not moving

        if (player == null)
        {
            float closestDist = Mathf.Infinity;
            GameObject[] players = GameObject.FindGameObjectsWithTag("player");

            foreach (GameObject p in players)
            {
                float dist = Vector3.Distance(transform.position, p.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    player = p.transform;
                }
            }

            if (player == null)
            {
                Debug.LogError("No PlayerAgent found near this enemy.");
            }
        }
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);

            // µÃÇ¨ÊÍºÃÐÂÐâ¨ÁµÕ
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
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
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
        Destroy(gameObject); // ÈÑµÃÙËÒÂä»àÁ×èÍ¡Ñ´¼ÙéàÅè¹
    }
}
