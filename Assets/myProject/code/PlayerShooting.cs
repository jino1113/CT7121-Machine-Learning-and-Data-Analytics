using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootCooldown = 0.5f;
    public PlayerAgent agent;

    private float lastShootTime;

    public void TryShoot(float input)
    {
        if (input <= 0.5f) return;

        if (Time.time - lastShootTime < shootCooldown)
            return;

        lastShootTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.agent = this.agent;
        }
    }
}
