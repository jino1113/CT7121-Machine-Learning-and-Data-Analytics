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

    public Transform target; // Target to move toward
    public Transform collectible;
    private PlayerShooting shooter; // Bullet shooting system

    public float detectRadius = 10f;         // Radius for detection
    public float detectDistance = 20f;       // How far to cast
    public LayerMask enemyLayer;             // Assign in Inspector to only detect enemies

    private float smoothMoveInput = 0f;
    private float smoothStrafeInput = 0f;
    private float smoothRotationInput = 0f;

    public float inputSmoothTime = 0.1f; // Adjustable smoothing factor
    private float timeSinceSeenEnemy = 0f;

    bool CanWalkToCollectible()
    {
        if (collectible == null) return false;

        Vector3 start = transform.position + Vector3.up * 1.0f;
        Vector3 dir = (collectible.position - start).normalized;
        float distance = Vector3.Distance(start, collectible.position);

        if (Physics.Raycast(start, dir, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false;
            }
        }

        return true;
    }



    private Transform GetVisibleCollectible()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectDistance, LayerMask.GetMask("Collectible"));

        foreach (Collider hit in hits)
        {
            Vector3 direction = hit.transform.position - playerCamera.transform.position;
            float distance = Vector3.Distance(playerCamera.transform.position, hit.transform.position);

            if (Physics.Raycast(playerCamera.transform.position, direction.normalized, out RaycastHit rayHit, distance))
            {
                if (rayHit.collider == hit)
                {
                    return hit.transform;
                }
            }
        }

        return null;
    }

    private GameObject FindNearestCollectible()
    {
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible"); // ตรวจ tag ให้ตรง
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject c in collectibles)
        {
            float dist = Vector3.Distance(c.transform.position, currentPos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = c;
            }
        }
        return nearest;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void Initialize()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        shooter = GetComponent<PlayerShooting>(); 
    }

    public override void OnEpisodeBegin()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }

        transform.position = spawnpoint.transform.position;
        moveDirection = Vector3.zero;
        controller.Move(moveDirection * Time.deltaTime);

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.health = 3;
            health.UpdateHealthUI();
            if (health.playerUI != null)
                health.playerUI.SetActive(true);
        }

        // ค้นหา collectible ใกล้ที่สุด
        GameObject nearestCollectible = FindNearestCollectible();
        if (nearestCollectible != null)
        {
            collectible = nearestCollectible.transform;
        }
        else if (spawnpoint != null)
        {
            // ตรวจให้แน่ใจว่าไม่มี dummy ซ้ำ
            GameObject dummy = GameObject.Find("DummyCollectible");
            if (dummy == null)
            {
                dummy = new GameObject("DummyCollectible");
                dummy.transform.position = spawnpoint.transform.position + new Vector3(
                    Random.Range(-4f, 4f),
                    0f,
                    Random.Range(-4f, 4f)
                );
            }
            collectible = dummy.transform;
        }

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.ResetEnemies();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // position
        sensor.AddObservation(transform.localPosition);

        if (target != null)
        {
            sensor.AddObservation(target.localPosition);

            Vector3 toEnemy = target.localPosition - transform.localPosition;
            sensor.AddObservation(toEnemy.normalized); 
            sensor.AddObservation(toEnemy.magnitude);  
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        sensor.AddObservation(moveDirection);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // ตรวจสอบศัตรูและของสะสม
        target = EnemyVisibilityUtil.GetVisibleEnemy(transform, "Enemy", "Wall", detectDistance);
        bool seesEnemyClearly = target != null;

        Transform seenCollectible = GetVisibleCollectible();
        bool seesCollectible = seenCollectible != null;

        // รีเซ็ต collectible ถ้าโดนเก็บไปแล้ว
        if (collectible == null || !collectible.gameObject.activeInHierarchy)
        {
            collectible = seesCollectible ? seenCollectible : FindNearestCollectible()?.transform;
        }

        // ตรวจจับความเคลื่อนไหวเพื่อลดรางวัล
        Vector3 flatMovement = moveDirection;
        flatMovement.y = 0f;
        if (flatMovement.magnitude > 0.1f)
        {
            AddReward(-flatMovement.magnitude * 0.001f);
        }

        // รับค่า Input
        float rawMove = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawStrafe = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.ContinuousActions[2] > 0.5f;
        float rawRotation = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);
        bool isRunning = actions.ContinuousActions[4] > 0.5f;
        float shootInput = actions.ContinuousActions.Length > 5 ? actions.ContinuousActions[5] : 0f;

        float speed = isRunning ? runSpeed : walkSpeed;

        // ถ้าเห็นศัตรู
        if (seesEnemyClearly)
        {
            shootInput = 1f;

            // ถ้าเห็นของสะสม → เดินช้าๆ ไปเก็บ
            if (collectible != null && CanWalkToCollectible())
            {
                Vector3 dirToCollectible = (collectible.position - transform.position).normalized;
                rawMove = Vector3.Dot(transform.forward, dirToCollectible) * 0.2f;
                rawStrafe = Vector3.Dot(transform.right, dirToCollectible) * 0.2f;
            }

            // ไม่ต้องถอยหลัง ไม่ต้องดันออก ไม่ต้องขยับมั่วแล้ว
            timeSinceSeenEnemy += Time.deltaTime;
        }
        else
        {
            timeSinceSeenEnemy = 0f;

            // ถ้าไม่เห็นศัตรู เดินไปหา collectible ปกติ
            if (collectible != null && CanWalkToCollectible())
            {
                Vector3 dirToCollectible = (collectible.position - transform.position).normalized;
                rawMove = Vector3.Dot(transform.forward, dirToCollectible);
                rawStrafe = Vector3.Dot(transform.right, dirToCollectible);
            }
        }

        // ยิง
        if (shooter != null && seesEnemyClearly)
            shooter.TryShoot(shootInput);

        // อินพุตลื่นไหล
        smoothMoveInput = Mathf.Lerp(smoothMoveInput, rawMove, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothStrafeInput = Mathf.Lerp(smoothStrafeInput, rawStrafe, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothRotationInput = Mathf.Lerp(smoothRotationInput, rawRotation, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));

        // เคลื่อนที่
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

        // หมุน
        transform.Rotate(0, smoothRotationInput * rotationSpeed * Time.deltaTime, 0);

        // หากอยู่ใกล้ศัตรูและอยู่ข้างหน้าให้ถอย
        if (target != null)
        {
            Vector3 dirToEnemy = (target.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, target.position);
            float forwardDot = Vector3.Dot(transform.forward, dirToEnemy);

            if (distanceToEnemy < 10f && forwardDot > 0.5f)
            {
                rawMove = -1f;
                rawStrafe = 0f;
            }
        }

        // รางวัลเพิ่มตามเวลา
        AddReward(0.001f * Time.deltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;

        c[0] = Input.GetAxis("Vertical");             // Move forward/backward
        c[1] = Input.GetAxis("Horizontal");           // Move left/right
        c[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f; // Jump
        c[3] = Input.GetAxis("Mouse X");              // Rotate camera
        c[4] = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f; // Run
        c[5] = Input.GetMouseButton(0) ? 1f : 0f;     // Shoot
    }
}
