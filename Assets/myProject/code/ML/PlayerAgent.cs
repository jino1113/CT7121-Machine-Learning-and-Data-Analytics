using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(CharacterController))]
public class PlayerAgent : Agent
{
    public CharacterController controller;
    public Camera playerCamera;
    public Transform collectible;
    public Transform targetEnemy;
    public GameObject spawnpoint;
    private PlayerShooting shooter;

    public float walkSpeed = 6f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float rotationSpeed = 200f;

    private Vector3 moveDirection = Vector3.zero;

    private float smoothMoveInput = 0f;
    private float smoothStrafeInput = 0f;
    private float smoothRotationInput = 0f;

    public float inputSmoothTime = 0.1f;

    public PlayerHealth playerHealth;

    private Transform cachedCollectible;
    private float collectibleSwitchThreshold = 2.5f;

    public LayerMask collectibleLayer;
    public LayerMask enemyLayer;
    //
    [Header("Debug / Spinbot")]
    public bool useSpinBot;

    private Vector3 lastPosition;
    private int idleStepCount = 0;
    public int maxIdleSteps = 500;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor in center
        Cursor.visible = false;                   // Hide cursor
    }

    public override void Initialize()
    {
        lastPosition = transform.position;
        idleStepCount = 0;

        controller = GetComponent<CharacterController>();
        shooter = GetComponent<PlayerShooting>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    public override void OnEpisodeBegin()
    {
        lastPosition = transform.position;
        idleStepCount = 0;

        SensorComponent[] sensors = GetComponentsInChildren<SensorComponent>();
        foreach (var s in sensors)
        {
            Debug.Log("Sensor: " + s.GetType().Name);
        }

        controller.enabled = false;
        transform.position = spawnpoint != null ? spawnpoint.transform.position : Vector3.zero;
        moveDirection = Vector3.zero;
        controller.enabled = true;

        // Reset player health
        playerHealth.playerhealth = 5;
        playerHealth.UpdateHealthUI();

        if (playerHealth.playerUI != null)
            playerHealth.playerUI.SetActive(true);

        // Reset collectible UI
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.ResetCollectible();
        }

        // Reset kill UI
        if (KillCoutManager.Instance != null)
        {
            KillCoutManager.Instance.ResetKillCount();
        }

        // Reset wave manager
        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            wm.ResetWaves();
        }

        //// Randomize collectible position
        //if (collectible != null)
        //{
        //    collectible.localPosition = new Vector3(
        //        Random.Range(-4f, 4f),
        //        0.5f,
        //        Random.Range(-4f, 4f)
        //    );
        //}

        //// Randomize enemy position
        //if (targetEnemy != null)
        //{
        //    targetEnemy.localPosition = new Vector3(
        //        Random.Range(-4f, 4f),
        //        0.5f,
        //        Random.Range(-4f, 4f)
        //    );
        //}
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        if (collectible != null)
        {
            sensor.AddObservation(collectible.localPosition);
            sensor.AddObservation((collectible.localPosition - transform.localPosition).normalized);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, collectible.localPosition));
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }

        if (targetEnemy != null)
        {
            sensor.AddObservation(targetEnemy.localPosition);
            sensor.AddObservation((targetEnemy.localPosition - transform.localPosition).normalized);
            sensor.AddObservation(Vector3.Distance(transform.localPosition, targetEnemy.localPosition));
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
        // For Smooth Movement Only!!!!
        float rawMove = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawStrafe = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.ContinuousActions[2] > 0.5f;

        // useSpinBot
        if (useSpinBot)
        {
            smoothRotationInput = 1f;
        }
        else
        {
            float rawRotation = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);
            smoothRotationInput = Mathf.Lerp(smoothRotationInput, rawRotation, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        }

        float shootInput = actions.ContinuousActions.Length > 4 ? actions.ContinuousActions[4] : 0f;


        smoothMoveInput = Mathf.Lerp(smoothMoveInput, rawMove, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothStrafeInput = Mathf.Lerp(smoothStrafeInput, rawStrafe, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        float yVelocity = moveDirection.y;

        moveDirection = (forward * smoothMoveInput + right * smoothStrafeInput) * walkSpeed;
        moveDirection.y = yVelocity;

        collectible = GetCachedNearestCollectible();
        targetEnemy = FindNearestEnemy();

        HandleLockOnRotation();

        // Check if hitting a wall
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = moveDirection.normalized;
        if (Physics.Raycast(origin, direction, out hit, 0.7f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                SetReward(-0.5f);
                EndEpisode();
                return;
            }
        }

        if (controller.isGrounded && jump)
            moveDirection.y = jumpPower;
        else if (!controller.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
        transform.Rotate(0f, smoothRotationInput * rotationSpeed * Time.deltaTime, 0f);

        float reward = 0f;

        if (shooter != null)
        {
            shooter.TryShoot(shootInput);
        }

        if (collectible != null)
        {
            float dist = Vector3.Distance(transform.position, collectible.position);

            if (dist < 1.2f)
            {
                reward += 1f; 
            }

            float distanceReward = Mathf.Clamp01(1f - dist / 10f); 
            reward += distanceReward * 0.05f;
        }


        Vector3 flatMovement = moveDirection;
        flatMovement.y = 0;
        if (flatMovement.magnitude > 0.1f)
        {
            reward -= flatMovement.magnitude * 0.0005f;
        }

        if (playerHealth != null && playerHealth.playerhealth <= 0)
        {
            reward -= 2f;
            EndEpisode();
        }

        if (Vector3.Distance(transform.position, lastPosition) < 0.05f)
        {
            idleStepCount++;
        }
        else
        {
            idleStepCount = 0;
        }
        lastPosition = transform.position;

        if (idleStepCount >= maxIdleSteps)
        {
            SetReward(-0.3f);
            EndEpisode();
        }

        AddReward(reward);
    }

    private Transform GetCachedNearestCollectible()
    {
        if (cachedCollectible == null || !cachedCollectible.gameObject.activeInHierarchy ||
            Vector3.Distance(transform.position, cachedCollectible.position) > collectibleSwitchThreshold)
        {
            cachedCollectible = FindNearestCollectible();
        }
        return cachedCollectible;
    }

    // Find the nearest collectible in the scene
    private Transform FindNearestCollectible()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 30f, collectibleLayer);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider c in hits)
        {
            float dist = Vector3.Distance(transform.position, c.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = c.transform;
            }
        }
        return nearest;
    }

    // Find the nearest enemy in the scene
    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 30f, enemyLayer);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider c in hits)
        {
            float dist = Vector3.Distance(transform.position, c.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = c.transform;
            }
        }
        return nearest;
    }

    // Lock-On
    private void HandleLockOnRotation()
    {
        if (targetEnemy == null) return;

        // Smoothly rotate character toward the enemy
        Vector3 direction = (targetEnemy.position - transform.position).normalized;
        direction.y = 0f; // Ignore vertical rotation

        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Give reward if camera is well-aligned with the enemy
        if (playerCamera != null)
        {
            Vector3 viewDir = playerCamera.transform.forward;
            Vector3 toEnemy = (targetEnemy.position - playerCamera.transform.position).normalized;

            float dot = Vector3.Dot(viewDir, toEnemy); // 1 = perfect aim
            float alignmentReward = Mathf.Clamp01(dot);
            AddReward(alignmentReward * 0.01f); // Small reward for better aim
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var c = actionsOut.ContinuousActions;
        c[0] = Input.GetAxis("Vertical");
        c[1] = Input.GetAxis("Horizontal");
        c[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        c[3] = Input.GetAxis("Mouse X");
        c[4] = Input.GetMouseButton(0) ? 1f : 0f;
    }
}