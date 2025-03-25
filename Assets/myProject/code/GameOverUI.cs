using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject uiPanel;               // ���ŷ���ʴ�������Т�ͤ��� / The UI panel
    public TMP_Text messageText;             // ��ͤ��������ʴ� / Message shown on game over

    [Header("Config")]
    [TextArea]
    public string defaultMessage = "Game Over! You Died."; // ��ͤ���������� / Default message

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false); // ��͹ UI �͹������� / Hide UI at start
    }

    /// <summary>
    /// ���¡����ͼ����蹵�� / Call this when the player dies
    /// </summary>
    public void ShowGameOver()
    {
        Time.timeScale = 0f; // ��ش���� / Pause time

        if (uiPanel != null)
            uiPanel.SetActive(true);

        if (messageText != null)
            messageText.text = defaultMessage; // ���ͤ����ҡ Inspector

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// ���� Retry / Retry button
    /// </summary>
    public void RetryGame()
    {
        Time.timeScale = 1f; // ��Ѻ�һ��� / Resume time

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// ���� Quit / Quit button
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f; // ���͡ó� freeze ����
        Application.Quit(); // �͡�ҡ��

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // ��ش� Editor
        #endif
    }
}
