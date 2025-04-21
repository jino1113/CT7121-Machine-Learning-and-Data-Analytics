using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner[] spawners;
    public TMP_Text waveText;

    public int enemiesPerWave = 10;  // How many kills to advance to next wave
    private int currentWave = 1;
    private int enemiesKilled = 0;

    void Start()
    {
        UpdateWaveText(); // Show wave at beginning
        StartNewWave();
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;

        if (enemiesKilled >= enemiesPerWave)
        {
            enemiesKilled = 0;
            currentWave++;
            UpdateWaveText(); // This must be here
            StartNewWave();
        }
    }

    void StartNewWave()
    {
        foreach (var spawner in spawners)
        {
            spawner.maxEnemies += 2; // Optional: increase difficulty
            spawner.enabled = true;  // Just in case you pause them between waves
        }

        //Debug.Log("Wave " + currentWave + " started!");
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
            //Debug.Log("Updated Wave Text to: " + currentWave);
        }
        else
        {
            Debug.LogWarning("Wave Text is NOT assigned!");
        }
    }

    public void ResetWaves()
    {
        currentWave = 1;
        enemiesKilled = 0;
        UpdateWaveText();
    }
}
