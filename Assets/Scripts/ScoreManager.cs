using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreTMP;

    private int score = 0;
    public int highScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
        }
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreTMP != null) scoreTMP.text = score.ToString("D6");
    }
}
