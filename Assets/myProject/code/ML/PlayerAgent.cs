using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(CharacterController))]
public class PlayerAgent : Agent
{
    public CharacterController controller;
    public Camera playerCamera;

    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float rotationSpeed = 200f;
    public GameObject spawnpoint;

    private Vector3 moveDirection = Vector3.zero;

    //private float rotationX = 0;

    public Transform target; // เป้าหมาย / Target to move toward

    private PlayerShooting shooter; // ระบบยิงกระสุน / Bullet shooting system

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // ล็อกเมาส์ตรงกลางหน้าจอ / Lock the cursor in the center
        Cursor.visible = false; // ซ่อนเมาส์ / Hide the cursor
    }

    public override void Initialize()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        shooter = GetComponent<PlayerShooting>(); // ดึง component การยิง / Get the shooting component
    }

    public override void OnEpisodeBegin()
    {
        // เปิดใช้งาน CharacterController ใหม่ (บางครั้งถูกปิดตอนตาย) 
        // Re-enable CharacterController (may have been disabled when dying)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }

        // ย้ายผู้เล่นกลับไปยังตำแหน่งเริ่มต้น (spawnpoint)
        // Move agent back to spawnpoint position
        transform.position = spawnpoint.transform.position;

        // รีเซ็ตเลือดของผู้เล่นและ UI HP
        // Reset player HP and UI
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.health = 3;                    // กำหนดเลือดใหม่ / Reset HP
            health.UpdateHealthUI();              // อัปเดตข้อความ HP / Update HP text
            if (health.playerUI != null)
                health.playerUI.SetActive(true);  // แสดงกล่อง HP UI / Show HP UI
        }

        // สุ่มตำแหน่งใหม่ให้กับเป้าหมาย โดยอิงจาก spawnpoint
        // Reset target position near the spawnpoint
        if (target != null && spawnpoint != null)
        {
            Vector3 basePos = spawnpoint.transform.position;
            target.position = new Vector3(
                basePos.x + Random.Range(-4f, 4f), // สุ่ม X / Randomize X
                basePos.y,                         // คงค่า Y เดิม / Keep same Y
                basePos.z + Random.Range(-4f, 4f)  // สุ่ม Z / Randomize Z
            );
        }

        // รีเซ็ตศัตรูในฉาก (ถ้ามี EnemySpawner อยู่ใน scene)
        // Reset enemies (if EnemySpawner exists)
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.ResetEnemies();
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // ตำแหน่งของ Agent / Agent position
        sensor.AddObservation(moveDirection);           // ความเร็ว Agent / Current velocity
        sensor.AddObservation(target != null ? target.localPosition : Vector3.zero); // ตำแหน่งเป้าหมาย / Target position
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // รับข้อมูลจาก Neural Network หรือ Heuristic (Manual Input)
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);     // เดินหน้า/ถอยหลัง / Move forward/backward
        float strafeInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);   // เดินซ้าย/ขวา / Strafe left/right
        bool jump = actions.ContinuousActions[2] > 0.5f;                          // กระโดด / Jump
        float rotationInput = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f); // หมุนซ้าย/ขวา / Rotate left/right
        bool isRunning = actions.ContinuousActions[4] > 0.5f;                     // วิ่ง / Run
        float shootInput = actions.ContinuousActions.Length > 5 ? actions.ContinuousActions[5] : 0f; // ยิง / Shoot

        // ความเร็วขึ้นอยู่กับว่ากำลังกดวิ่งหรือไม่ / Choose walk or run speed
        float speed = isRunning ? runSpeed : walkSpeed;

        // แปลงทิศทางตามการหมุนของตัวละคร / Transform directions to world space
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float movementY = moveDirection.y; // เก็บความเร็วแกน Y ไว้ก่อน (แรงโน้มถ่วง)

        // สร้างเวกเตอร์การเคลื่อนที่ / Calculate horizontal movement
        moveDirection = (forward * moveInput + right * strafeInput) * speed;
        moveDirection.y = movementY; // คืนค่าความเร็วแกน Y กลับมา

        // กระโดด (เฉพาะตอนอยู่บนพื้น) / Jump only when grounded
        if (controller.isGrounded && jump)
        {
            moveDirection.y = jumpPower;
        }

        // ใช้แรงโน้มถ่วง / Apply gravity
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // เคลื่อนที่ด้วย CharacterController / Move the player
        controller.Move(moveDirection * Time.deltaTime);

        // หมุนตัวละครซ้าย-ขวา / Rotate player along Y-axis
        transform.Rotate(0, rotationInput * rotationSpeed * Time.deltaTime, 0);

        // ยิงกระสุน (ถ้ามีคำสั่ง) / Shoot bullet if commanded
        if (shooter != null)
        {
            shooter.TryShoot(shootInput);
        }

        // ให้รางวัลถ้าอยู่ใกล้เป้าหมาย / Reward for approaching target
        if (target != null)
        {
            float distance = Vector3.Distance(transform.localPosition, target.localPosition);
            AddReward(-distance / 1000f); // ยิ่งใกล้ยิ่งรางวัลน้อย (ค่าติดลบ) / Closer = less penalty
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;

        c[0] = Input.GetAxis("Vertical");             // เดินหน้า/ถอยหลัง / Move forward/backward
        c[1] = Input.GetAxis("Horizontal");           // เดินซ้าย/ขวา / Move left/right
        c[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f; // กระโดด / Jump
        c[3] = Input.GetAxis("Mouse X");              // หมุนกล้อง / Rotate camera
        c[4] = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f; // วิ่ง / Run
        c[5] = Input.GetMouseButton(0) ? 1f : 0f;     // ยิง / Shoot
    }
}
