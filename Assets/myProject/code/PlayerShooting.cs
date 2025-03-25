using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public float shootCooldown = 0.3f; // ความถี่ในการยิง (วินาที) / Time between shots
    private float lastShootTime = 0f;  // เวลาที่ยิงครั้งล่าสุด / Last time a shot was fired

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePoint.forward * bulletSpeed;
        Destroy(bullet, 2f);
        lastShootTime = Time.time; // อัปเดตเวลาที่ยิงล่าสุด / Update last shoot time
    }

    public void TryShoot(float value)
    {
        if (value > 0.5f && Time.time > lastShootTime + shootCooldown)
        {
            Shoot(); // ยิงถ้าพร้อมและ cooldown ครบ / Shoot if ready
        }
    }
}
