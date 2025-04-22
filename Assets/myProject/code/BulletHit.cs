using UnityEngine;

public class BulletHit : MonoBehaviour
{
    public PlayerAgent agent;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            agent.AddReward(1f); 
            Destroy(collision.gameObject);
        }
        else
        {
            agent.AddReward(-0.5f); 
        }

        Destroy(gameObject);
    }
}