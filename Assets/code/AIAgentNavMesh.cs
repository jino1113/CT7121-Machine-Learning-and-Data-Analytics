using UnityEngine;
using UnityEngine.AI;

public class AIAgentNavMesh : MonoBehaviour
{
    public Transform goal; // เป้าหมายที่ AI จะเดินไปหา The goal that AI will walk towards

    private NavMeshAgent agent;

    private void Start()
    {
        // ดึง Component NavMeshAgent Pull the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // ตั้งค่าเป้าหมายเริ่มต้น Set default goals
        agent.SetDestination(goal.position);
    }

    private void Update()
    {
        // ตรวจสอบว่าถึงเป้าหมายหรือยัง Check if the target has been reached
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Debug.Log("Reached the goal!");
        }
    }
}
