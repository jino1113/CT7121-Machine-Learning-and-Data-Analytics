using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform player; // ตัวละครที่ต้องการให้กล้องติดตาม
    public Vector3 offset = new Vector3(0, 5, -5); // ตำแหน่งกล้องห่างจากผู้เล่น
    public float smoothSpeed = 5f; // ความเร็วในการเคลื่อนที่ของกล้อง

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(player); // ให้กล้องหันไปมองผู้เล่น
        }
    }
}
