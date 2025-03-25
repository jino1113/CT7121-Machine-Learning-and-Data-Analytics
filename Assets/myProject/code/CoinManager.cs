using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public TMP_Text coinText;
    private int coins = 0;

    private void Awake()
    {
        UpdateCoinUI();
        AddCoin(0);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCoin(int amount)
    {
        coins += amount;
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coin: " + coins;
            Debug.Log("Coin UI Updated to: " + coinText.text);
        }
        else
        {
            Debug.LogWarning("coinText is null!");
        }
    }
}
