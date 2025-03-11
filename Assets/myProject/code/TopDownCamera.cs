using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform player; // ����Ф÷���ͧ��������ͧ�Դ���
    public Vector3 offset = new Vector3(0, 5, -5); // ���˹觡��ͧ��ҧ�ҡ������
    public float smoothSpeed = 5f; // ��������㹡������͹���ͧ���ͧ

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(player); // �����ͧ�ѹ��ͧ������
        }
    }
}
