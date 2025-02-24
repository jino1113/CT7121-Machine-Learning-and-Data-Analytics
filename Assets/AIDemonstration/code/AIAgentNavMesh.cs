using UnityEngine;
using UnityEngine.AI;

public class AIAgentNavMesh : MonoBehaviour
{
    public Transform goal; // ������·�� AI ���Թ��� The goal that AI will walk towards

    private NavMeshAgent agent;

    private void Start()
    {
        // �֧ Component NavMeshAgent Pull the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // ��駤���������������� Set default goals
        agent.SetDestination(goal.position);
    }

    private void Update()
    {
        // ��Ǩ�ͺ��Ҷ֧������������ѧ Check if the target has been reached
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Debug.Log("Reached the goal!");
        }
    }
}
