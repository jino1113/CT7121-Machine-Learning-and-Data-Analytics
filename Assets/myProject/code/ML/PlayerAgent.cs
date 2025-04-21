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
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;                  
    }

    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        shooter = GetComponent<PlayerShooting>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    public override void OnEpisodeBegin()
    {
        controller.enabled = false;
        transform.position = spawnpoint != null ? spawnpoint.transform.position : Vector3.zero;
        moveDirection = Vector3.zero;
        controller.enabled = true;

        WaveManager wm = FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            wm.ResetWaves();
        }

        if (collectible != null)
        {
            collectible.localPosition = new Vector3(
                Random.Range(-4f, 4f),
                0.5f,
                Random.Range(-4f, 4f)
            );
        }

        if (targetEnemy != null)
        {
            targetEnemy.localPosition = new Vector3(
                Random.Range(-4f, 4f),
                0.5f,
                Random.Range(-4f, 4f)
            );
        }
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
        float rawMove = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rawStrafe = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.ContinuousActions[2] > 0.5f;
        float rawRotation = Mathf.Clamp(actions.ContinuousActions[3], -1f, 1f);
        float shootInput = actions.ContinuousActions.Length > 4 ? actions.ContinuousActions[4] : 0f;

        smoothMoveInput = Mathf.Lerp(smoothMoveInput, rawMove, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothStrafeInput = Mathf.Lerp(smoothStrafeInput, rawStrafe, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));
        smoothRotationInput = Mathf.Lerp(smoothRotationInput, rawRotation, 1 - Mathf.Exp(-Time.deltaTime / inputSmoothTime));

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        float yVelocity = moveDirection.y;

        moveDirection = (forward * smoothMoveInput + right * smoothStrafeInput) * walkSpeed;
        moveDirection.y = yVelocity;

        // Check if hitting a wall
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = moveDirection.normalized;
        if (Physics.Raycast(origin, direction, out hit, 0.7f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                SetReward(-1f);
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

        if (shooter != null)
        {
            shooter.TryShoot(shootInput);
        }

        float reward = 0f;

        if (collectible != null)
        {
            float dist = Vector3.Distance(transform.position, collectible.position);
            if (dist < 1.2f)
            {
                reward += 1f;
                EndEpisode();
            }
        }

        Vector3 flatMovement = moveDirection; flatMovement.y = 0;
        if (flatMovement.magnitude > 0.1f)
        {
            reward -= flatMovement.magnitude * 0.001f;
        }

        if (transform.position.y < -1f)
        {
            reward -= 1f;
            EndEpisode();
        }

        if(playerHealth.playerhealth == 0)
        {
            reward -= 5f;
            EndEpisode();
        }

        AddReward(reward);
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
