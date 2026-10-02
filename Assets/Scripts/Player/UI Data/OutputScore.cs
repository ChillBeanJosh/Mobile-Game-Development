using System;
using TMPro;
using UnityEngine;

public class OutputScore : MonoBehaviour
{
    [Header("Score Output: ")]
    [SerializeField] private TextMeshProUGUI scoreOutput;


    private void Update()
    {
        scoreOutput.text = $"<color=yellow>Score: {CoinCollector.collectedCoins}</color>";
    }
}
