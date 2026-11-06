using UnityEngine;
using TMPro;

public class UIPositionInfo : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CoinCollector coinCollector;
    [Space]


    [Header("UI Position Info: ")]
    [SerializeField] private TextMeshProUGUI positionData;

    [Header("UI Health Info: ")]
    [SerializeField] private TextMeshProUGUI healthData;

    [Header("UI Score Info: ")]
    [SerializeField] private TextMeshProUGUI scoreData;

    private void Update()
    {
        UpdatePlayerInfo();
        //UpdateScore();
    }

    private void UpdatePlayerInfo()
    {
        if (playerMovement != null && positionData != null)
        {
            // Clear and set the text fresh every frame using '=' instead of '+='
            string infoText = "";

            Vector3 playerVelocity = playerMovement.GetCurrentVelocity();
            float playerSpeed = playerVelocity.magnitude;
            infoText += $"<color=white>Player Speed: {playerSpeed:F2}</color>\n";

            if (playerHealth != null)
            {
                float currentScore = playerHealth.GetCurrentScore();
                infoText += $"<color=yellow>Current Score: {currentScore:F2}</color>\n";
            }

            // Assign the assembled text all at once
            positionData.text = infoText;
        }
    }

    private void UpdateScore()
    {
        if (coinCollector != null && scoreData != null)
        {
            scoreData.text = $"<color=yellow>Score: {coinCollector.GetCollectedCoins()}</color>\n";
        }
    }
}
