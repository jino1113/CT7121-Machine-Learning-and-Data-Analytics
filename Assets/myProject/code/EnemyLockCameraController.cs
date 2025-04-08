using UnityEngine;

public class EnemyLockCameraController : MonoBehaviour
{
    public Transform cameraTransform; // Drag your Camera here in Inspector
    public Transform playerTransform; // Drag your PlayerAgent here
    public string enemyTag = "Enemy"; // All enemies must have this tag
    public string wallTag = "Wall";
    public float rotationSpeed = 5f;
    public Transform gunTransform;

    void Update()
    {

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0 || cameraTransform == null) return;

        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            // Check line of sight first
            Vector3 direction = (enemy.transform.position - playerTransform.position).normalized;
            Vector3 origin = playerTransform.position + Vector3.up * 1.2f; // Slightly above ground
            Vector3 target = enemy.transform.position + Vector3.up * 1.2f;

            if (Physics.Linecast(origin, target, out RaycastHit hit))
            {
                if (hit.collider.CompareTag(wallTag))
                {
                    continue; // Enemy is behind wall — skip it
                }
            }

            float dist = Vector3.Distance(playerTransform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                closest = enemy.transform;
                minDistance = dist;
            }
        }

        if (closest != null)
        {
            // Rotate camera toward the enemy
            cameraTransform.LookAt(closest);

            // Smoothly rotate player (only Y-axis)
            Vector3 direction = (closest.position - playerTransform.position).normalized;
            direction.y = 0f; // Prevent tilting up/down

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Rotate gun to aim at the enemy
            if (gunTransform != null)
            {
                gunTransform.LookAt(closest.position + Vector3.up); // Adjust Y height if needed
            }
        }
    }
}

public static class EnemyVisibilityUtil
{
    public static Transform GetVisibleEnemy(Transform player, string enemyTag, string wallTag, float maxDistance)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            Vector3 origin = player.position + Vector3.up * 1.2f;
            Vector3 target = enemy.transform.position + Vector3.up * 1.2f;

            if (Physics.Linecast(origin, target, out RaycastHit hit))
            {
                if (hit.collider.CompareTag(wallTag))
                    continue;
            }

            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < minDistance && distance <= maxDistance)
            {
                closest = enemy.transform;
                minDistance = distance;
            }
        }

        return closest;
    }
}
