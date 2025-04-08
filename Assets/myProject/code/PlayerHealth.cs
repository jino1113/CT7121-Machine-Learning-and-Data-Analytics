using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int health = 5;

    public GameOverUI gameOverUI;     // UI Game Over
    public GameObject playerUI;       // กล่อง UI ที่ครอบ HP ทั้งหมด
    public TMP_Text healthText;       // ข้อความที่แสดง HP

    private BehaviorParameters behaviorParameters; // ตรวจสอบโหมด Heuristic

    void Start()
    {
        behaviorParameters = GetComponent<BehaviorParameters>(); // เก็บไว้ใช้
        UpdateHealthUI(); // แสดงค่าเริ่มต้น
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Player HP: " + health);

        var agent = GetComponent<Agent>();
        if (agent != null)
        {
            agent.AddReward(-1f); // ลดรางวัลเมื่อโดนโจมตี / Reduce rewards when attacked
        }

        UpdateHealthUI(); // อัปเดต UI HP

        if (health <= 0)
        {
            Debug.Log("Player Died!");
            agent.AddReward(-5f);

            if (agent != null)
            {
                agent.EndEpisode(); // จบ Episode ทันที

                if (behaviorParameters == null)
                    Debug.LogWarning("behaviorParameters is null");
                else
                    Debug.Log("BehaviorType: " + behaviorParameters.BehaviorType);

                if (behaviorParameters != null &&
                    behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly &&
                    gameOverUI != null)
                {
                    Debug.Log("ShowGameOver() เรียกแล้ว");
                    gameOverUI.ShowGameOver();
                }
            }

            //if (playerUI != null)
            //    playerUI.SetActive(false); // ซ่อน UI
        }
    }

    public void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "HP: " + health.ToString();
    }
}
