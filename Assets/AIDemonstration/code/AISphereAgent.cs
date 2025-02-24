using Unity.MLAgents; // ใช้ ML-Agents ใน Unity / Using ML-Agents in Unity
using Unity.MLAgents.Sensors; // สำหรับเซ็นเซอร์ที่เก็บข้อมูล / For sensors that collect data
using Unity.MLAgents.Actuators; // สำหรับตัวกระทำ (Actions) / For actions (actuators)
using UnityEngine; // คลังพื้นฐานของ Unity / Basic Unity library

public class AISphereAgent : Agent // คลาสนี้สืบทอดจาก Agent ซึ่งเป็นคลาสพื้นฐานของ ML-Agents / This class inherits from Agent, the base class for ML-Agents
{
    public Transform goalTransform; // ตัวแปรสำหรับเก็บตำแหน่งของเป้าหมาย / Variable to store the target's position

    public override void OnEpisodeBegin()
    {
        // รีเซ็ตตำแหน่งของเอเจนต์และเป้าหมายเมื่อเริ่ม Episode ใหม่
        // Reset the positions of the agent and target when a new episode begins
        transform.localPosition = new Vector3(Random.Range(-4, 4), 0.5f, Random.Range(-4, 4));
        goalTransform.localPosition = new Vector3(Random.Range(-4, 4), 0.5f, Random.Range(-4, 4));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // เก็บข้อมูลตำแหน่งของเอเจนต์และเป้าหมายเพื่อส่งให้ Neural Network
        // Collect the agent and target positions to send to the neural network
        sensor.AddObservation(transform.localPosition); // ตำแหน่งของเอเจนต์ / Agent's position
        sensor.AddObservation(goalTransform.localPosition); // ตำแหน่งของเป้าหมาย / Target's position
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // รับคำสั่งจาก Neural Network เพื่อควบคุมการเคลื่อนที่
        // Receive commands from the neural network to control movement
        float moveX = actions.ContinuousActions[0]; // รับคำสั่งการเคลื่อนที่ในแกน X / Movement command on the X axis
        float moveZ = actions.ContinuousActions[1]; // รับคำสั่งการเคลื่อนที่ในแกน Z / Movement command on the Z axis

        // เคลื่อนที่ตามคำสั่งที่ได้รับ
        // Move according to the received commands
        transform.Translate(new Vector3(moveX, 0, moveZ) * Time.deltaTime * 2);

        // คำนวณระยะห่างระหว่างเอเจนต์กับเป้าหมาย
        // Calculate the distance between the agent and the target
        float distanceToGoal = Vector3.Distance(transform.localPosition, goalTransform.localPosition);

        if (distanceToGoal < 1.5f) // ถ้าระยะใกล้เป้าหมาย / If the distance is close to the target
        {
            SetReward(1.0f); // ให้รางวัล 1.0 / Give a reward of 1.0
            EndEpisode(); // จบ Episode / End the episode
        }
        else
        {
            SetReward(-0.01f); // ลดรางวัลเล็กน้อยสำหรับแต่ละขั้นตอน / Slightly reduce the reward for each step
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // ให้ผู้ใช้ควบคุมด้วยมือสำหรับการทดสอบ
        // Allow manual control for testing
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // รับค่าแกน X จากคีย์บอร์ด / Get the X-axis value from the keyboard
        continuousActionsOut[1] = Input.GetAxis("Vertical"); // รับค่าแกน Z จากคีย์บอร์ด / Get the Z-axis value from the keyboard
    }
}
