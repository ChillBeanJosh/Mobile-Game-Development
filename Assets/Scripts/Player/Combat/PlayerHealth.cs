using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Level Progression:")]
    [SerializeField, Min(1)] private int currentLevelNumber = 1;
    [SerializeField] private float speedLevelClearThreshold = 1000f;
    private bool hasEndedRun = false;
    [Space]
    [SerializeField] private float currentScore = 0f;
    private float maxSpeed = 0f;

    private void Update()
    {
        if (hasEndedRun) return;
        float currentSpeed = playerMovement.GetCurrentVelocity().magnitude;
        bool movementHasBegun = playerMovement.GetHasAchievedSpeed();

        //Movement Trigger
        if (movementHasBegun && currentSpeed > 0f)
        {
            //Score Trigger: Takes current speed over time, multiplies it by a score multiplier, and adds it to the current score.
            currentScore += currentSpeed * (1 + StatusUpgrade.Instance.GetUpgradedScoreMultiplier()) * Time.deltaTime;

            //Store Trigger: Keeps track of the highest speed achieved during the run for display on the score output screen.
            if (currentSpeed > maxSpeed)
            {
                maxSpeed = currentSpeed;
            }
        }

        //Death Trigger
        if (currentSpeed <= 0f && movementHasBegun)
        {
            hasEndedRun = true;
            SaveEarnings();
            OnDie();
            return;
        }

        //Level Clear Trigger
        if (currentSpeed >= speedLevelClearThreshold)
        {
            hasEndedRun = true;
            SaveEarnings();
            OnClear();
            return;
        }
    }

    private void SaveEarnings()
    {
        //Load existing total money from PlayerPrefs:
        float existingMoney = PlayerPrefs.GetFloat(StatusUpgrade.MoneySaveKey, 0f);

        //Add current score to the existing total money:
        float newTotalMoney = existingMoney + currentScore;

        //Store and save the new total money back into PlayerPrefs:
        PlayerPrefs.SetFloat(StatusUpgrade.MoneySaveKey, newTotalMoney);
        PlayerPrefs.Save(); 
    }

    private void SaveRunData()
    {
        //Store the accumulated score from the current run:
        PlayerPrefs.SetFloat("LastRunScore", currentScore);

        //Store the peak speed achieved during the current run:
        PlayerPrefs.SetFloat("LastRunPeakSpeed", maxSpeed);

        //Save all stored data to PlayerPrefs:
        PlayerPrefs.Save();
    }

    private void OnDie()
    {
        SaveRunData();
        //playerInput.ToggleScreenLock();
        sceneManager.Instance.LoadGameOver();
    }

    private void OnClear()
    {
        SaveRunData();
        sceneManager.Instance.CompleteLevel(currentLevelNumber);
        //playerInput.ToggleScreenLock();
        //sceneManager.Instance.LoadLevelClear();
    }

    public float GetCurrentScore() => currentScore;
}
