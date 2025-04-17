using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI collectibleText;
    private int collectibleCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCollectible(string collectibleName)
    {
        collectibleCount++;
        Debug.Log($"collectible: {collectibleCount}");
        if (collectibleText != null)
            collectibleText.text = $"collectible: {collectibleCount}";
    }
}
