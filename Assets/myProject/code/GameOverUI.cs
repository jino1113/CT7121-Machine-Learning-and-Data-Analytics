using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiPanel;               // พาเนลที่แสดงปุ่มและข้อความ / The UI panel
    public TMP_Text messageText;             // ข้อความที่จะแสดง / Message shown on game over

    [Header("Config")]
    [TextArea]
    public string defaultMessage = "Game Over! You Died."; // ข้อความเริ่มต้น / Default message

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false); // ซ่อน UI ตอนเริ่มเกม / Hide UI at start
    }

    /// <summary>
    /// เรียกเมื่อผู้เล่นตาย / Call this when the player dies
    /// </summary>
    public void ShowGameOver()
    {
        Time.timeScale = 0f; // หยุดเวลา / Pause time

        if (uiPanel != null)
            uiPanel.SetActive(true);

        if (messageText != null)
            messageText.text = defaultMessage; // ใช้ข้อความจาก Inspector

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// ปุ่ม Retry / Retry button
    /// </summary>
    public void RetryGame()
    {
        Time.timeScale = 1f; // กลับมาปกติ / Resume time

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// ปุ่ม Quit / Quit button
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f; // เผื่อกรณี freeze อยู่
        Application.Quit(); // ออกจากเกม

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // หยุดใน Editor
        #endif
    }
}
