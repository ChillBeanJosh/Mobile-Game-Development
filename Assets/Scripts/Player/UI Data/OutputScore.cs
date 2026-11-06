using System;
using TMPro;
using UnityEngine;

public class OutputScore : MonoBehaviour
{
    [Header("Score Output References: ")]
    [SerializeField] private TextMeshProUGUI scoreOutput;
    [SerializeField] private TextMeshProUGUI peakSpeedOutput;
    [SerializeField] private TextMeshProUGUI totalCurrency;

    private const string MoneySaveKey = "TotalMoney";

    private void Start()
    {
        // 1. Retrieve stats saved during the run
        float finalScore = PlayerPrefs.GetFloat("LastRunScore", 0f);
        float peakSpeed = PlayerPrefs.GetFloat("LastRunPeakSpeed", 0f);
        float totalMoney = PlayerPrefs.GetFloat(MoneySaveKey, 0f);

        // 2. Assign to TextMeshPro components (formatting as integers or floats with decimals)
        if (scoreOutput != null)
        {
            scoreOutput.text = $"Score: {finalScore:F0}";
        }

        if (peakSpeedOutput != null)
        {
            peakSpeedOutput.text = $"Top Speed: {peakSpeed:F1}";
        }

        if (totalCurrency != null)
        {
            totalCurrency.text = $"Total Money: {totalMoney:F0}";
        }
    }
}
