using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using static UnityEditor.PlayerSettings.SplashScreen;

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

    public float detectRadius = 10f;         // Radius for detection
    public float detectDistance = 20f;       // How far to cast
    public LayerMask enemyLayer;             // Assign in Inspector to only detect enemies

    private float smoothMoveInput = 0f;
    private float smoothStrafeInput = 0f;
    private float smoothRotationInput = 0f;

    public float inputSmoothTime = 0.1f; // Adjustable smoothing factor

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // ล็อกเมาส์ตรงกลางหน้าจอ / Lock the cursor in the center
        Cursor.visible = false; // ซ่อนเมาส์ / Hide the cursor
        //enemy = GameObject.FindWithTag("Enemy")?.transform;
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
        moveDirection = Vector3.zero;
        controller.Move(moveDirection * Time.deltaTime);

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
        // ตำแหน่งของตัวเอง / Own position
        sensor.AddObservation(transform.localPosition);

        // ตำแหน่งศัตรู (enemy) — อย่าลืม assign ให้ถูกใน inspector
        if (target != null)
        {
            sensor.AddObservation(target.localPosition);

            // ระยะห่างถึงศัตรู
            Vector3 toEnemy = target.localPosition - transform.localPosition;
            sensor.AddObservation(toEnemy.normalized); // ทิศทาง
            sensor.AddObservation(toEnemy.magnitude);  // ระยะ
        }
        else
        {
            // กัน null
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        // ความเร็วของตัวเอง
        sensor.AddObservation(moveDirection);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        target = EnemyVisibilityUtil.GetVisibleEnemy(transform, "Enemy", "Wall", detectDistance);
        bool seesEnemyClearly = target != null;

        // Movement penalty
        Vector3 flatMovement = moveDirection;
        flatMovement.y = 0f; // Ignore vertical movement
        float movementAmount = flatMovement.magnitude;

        if (movementAmount > 0.1f)
        {
            AddReward(-movementAmount * 0.001f); // Penalize slight for moving too much
        }

        // Get raw inputs from actions
        float rawMove = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawStrafe = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.ContinuousActions[2] > 0.5f;
        float rawRotation = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);
        bool isRunning = actions.ContinuousActions[4] > 0.5f;
        float shootInput = actions.ContinuousActions.Length > 5 ? actions.ContinuousActions[5] : 0f;

        float speed = isRunning ? runSpeed : walkSpeed;

        if (seesEnemyClearly)
        {
            // Stop movement and shoot when enemy is seen
            rawMove = 0f;
            rawStrafe = 0f;
            shootInput = 1f; // Shoot continuously
        }

        // Shooting (shooting only happens when player sees the enemy)
        if (shooter != null && seesEnemyClearly)
            shooter.TryShoot(shootInput);

        // Smooth inputs
        smoothMoveInput = Mathf.Lerp(smoothMoveInput, rawMove, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothStrafeInput = Mathf.Lerp(smoothStrafeInput, rawStrafe, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothRotationInput = Mathf.Lerp(smoothRotationInput, rawRotation, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));

        // Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float movementY = moveDirection.y;

        moveDirection = (forward * smoothMoveInput + right * smoothStrafeInput) * speed;
        moveDirection.y = movementY;

        if (controller.isGrounded && jump)
            moveDirection.y = jumpPower;
        if (!controller.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);

        // Smooth rotation
        transform.Rotate(0, smoothRotationInput * rotationSpeed * Time.deltaTime, 0);

        // Reward shaping
        if (target != null)
        {
            Vector3 dirToEnemy = (target.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, target.position);

            // Check if enemy is in front
            float forwardDot = Vector3.Dot(transform.forward, dirToEnemy);

            if (distanceToEnemy < 10f && forwardDot > 0.5f)
            {
                // If enemy is close and in front, move backward
                rawMove = -1f;
                rawStrafe = 0f;
            }
        }

        // อยู่รอดได้ = ได้รางวัลเล็กน้อย
        AddReward(0.001f * Time.deltaTime);
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
