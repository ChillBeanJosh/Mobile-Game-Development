using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float speedLevelClearThreshold = 10000f;

    private void Update()
    {
        float currentSpeed = playerMovement.GetCurrentVelocity().magnitude;
        bool hasAchievedSpeed = playerMovement.GetHasAchievedSpeed();

        if (currentSpeed <= 0f && hasAchievedSpeed)
        {
            OnDie();
        }


        if (currentSpeed >= speedLevelClearThreshold)
        {
            OnClear();
        }
    }

    private void OnDie()
    {
        playerInput.ToggleScreenLock();
        sceneManager.Instance.LoadGameOver();
    }

    private void OnClear()
    {
        playerInput.ToggleScreenLock();
        sceneManager.Instance.LoadLevelClear();
    }
}
