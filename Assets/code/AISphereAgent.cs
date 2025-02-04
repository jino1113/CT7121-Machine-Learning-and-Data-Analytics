using Unity.MLAgents; // �� ML-Agents � Unity / Using ML-Agents in Unity
using Unity.MLAgents.Sensors; // ����Ѻ���������红����� / For sensors that collect data
using Unity.MLAgents.Actuators; // ����Ѻ��ǡ�з� (Actions) / For actions (actuators)
using UnityEngine; // ��ѧ��鹰ҹ�ͧ Unity / Basic Unity library

public class AISphereAgent : Agent // ���ʹ���׺�ʹ�ҡ Agent ����繤��ʾ�鹰ҹ�ͧ ML-Agents / This class inherits from Agent, the base class for ML-Agents
{
    public Transform goalTransform; // ���������Ѻ�纵��˹觢ͧ������� / Variable to store the target's position

    public override void OnEpisodeBegin()
    {
        // ���絵��˹觢ͧ��ਹ���������������������� Episode ����
        // Reset the positions of the agent and target when a new episode begins
        transform.localPosition = new Vector3(Random.Range(-4, 4), 0.5f, Random.Range(-4, 4));
        goalTransform.localPosition = new Vector3(Random.Range(-4, 4), 0.5f, Random.Range(-4, 4));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // �红����ŵ��˹觢ͧ��ਹ��������������������� Neural Network
        // Collect the agent and target positions to send to the neural network
        sensor.AddObservation(transform.localPosition); // ���˹觢ͧ��ਹ�� / Agent's position
        sensor.AddObservation(goalTransform.localPosition); // ���˹觢ͧ������� / Target's position
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // �Ѻ����觨ҡ Neural Network ���ͤǺ����������͹���
        // Receive commands from the neural network to control movement
        float moveX = actions.ContinuousActions[0]; // �Ѻ����觡������͹����᡹ X / Movement command on the X axis
        float moveZ = actions.ContinuousActions[1]; // �Ѻ����觡������͹����᡹ Z / Movement command on the Z axis

        // ����͹���������觷�����Ѻ
        // Move according to the received commands
        transform.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * 2);

        // �ӹǳ������ҧ�����ҧ��ਹ��Ѻ�������
        // Calculate the distance between the agent and the target
        float distanceToGoal = Vector3.Distance(transform.localPosition, goalTransform.localPosition);

        if (distanceToGoal < 1.5f) // ����������������� / If the distance is close to the target
        {
            SetReward(1.0f); // ����ҧ��� 1.0 / Give a reward of 1.0
            EndEpisode(); // �� Episode / End the episode
        }
        else
        {
            SetReward(-0.01f); // Ŵ�ҧ�����硹�������Ѻ���Т�鹵͹ / Slightly reduce the reward for each step
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // �������Ǻ��������������Ѻ��÷��ͺ
        // Allow manual control for testing
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // �Ѻ���᡹ X �ҡ������� / Get the X-axis value from the keyboard
        continuousActionsOut[1] = Input.GetAxis("Vertical"); // �Ѻ���᡹ Z �ҡ������� / Get the Z-axis value from the keyboard
    }
}
