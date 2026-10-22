using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerMovement playerMovement;

    private void Update()
    {
        float currentSpeed = playerMovement.GetCurrentVelocity().magnitude;
        bool hasAchievedSpeed = playerMovement.GetHasAchievedSpeed();

        if (currentSpeed <= 0f && hasAchievedSpeed)
        {
            OnDie();
        }
    }

    private void OnDie()
    {
        playerInput.ToggleScreenLock();
        sceneManager.Instance.LoadGameOver();
    }
}
