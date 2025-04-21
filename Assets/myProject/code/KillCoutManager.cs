using TMPro;
using UnityEngine;

public class KillCoutManager : MonoBehaviour
{
    public static KillCoutManager Instance;

    public TMP_Text coinText;
    private int Kill = 0;

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
        Kill += amount;
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Kill: " + Kill;
            //Debug.Log("Coin UI Updated to: " + coinText.text);
        }
        else
        {
            Debug.LogWarning("coinText is null!");
        }
    }

    public void ResetKillCount()
    {
        Kill = 0;
        UpdateCoinUI();
    }
}
