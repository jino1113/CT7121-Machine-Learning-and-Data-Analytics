using UnityEngine;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance;

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
        // Debug.Log($"collectible: {collectibleCount}");
        if (collectibleText != null)
            collectibleText.text = $"collectibles: {collectibleCount}";
    }

    public void ResetCollectible()
    {
        collectibleCount = 0;
        if (collectibleText != null)
            collectibleText.text = "collectibles: 0";
    }

}
