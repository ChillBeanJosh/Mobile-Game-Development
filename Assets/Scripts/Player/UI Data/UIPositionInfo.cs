using UnityEngine;
using TMPro;

public class UIPositionInfo : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [Space]


    [Header("UI Position Info: ")]
    [SerializeField] private TextMeshProUGUI positionData;

    [Header("UI Health Info: ")]
    [SerializeField] private TextMeshProUGUI healthData;

    private void Update()
    {
        UpdatePlayerInfo();
        UpdatePlayerHealth();
    }

    private void UpdatePlayerInfo()
    {
        if (playerMovement != null && positionData != null)
        {
            Vector3 playerPosition = playerMovement.GetCurrentPosition();
            positionData.text = $"<color=yellow>Player Position: X: {playerPosition.x:F2}, Y: {playerPosition.y:F2}, Z: {playerPosition.z:F2}</color>\n";

            Vector3 playerVelocity = playerMovement.GetCurrentVelocity();
            positionData.text += $"<color=green>Player Velocity: X: {playerVelocity.x:F2}, Y: {playerVelocity.y:F2}, Z: {playerVelocity.z:F2}</color>\n";

            Vector3 playerAcceleration = playerMovement.GetTotalAcceleration();
            positionData.text += $"<color=red>Player Acceleration: X: {playerAcceleration.x:F2}, Y: {playerAcceleration.y:F2}, Z: {playerAcceleration.z:F2}</color>\n";

            float playerSpeed = playerVelocity.magnitude;
            positionData.text += $"<color=white>Player Speed: {playerSpeed:F2}</color>\n";

            bool isGrounded = playerMovement.GetGroundedStatus();
            positionData.text += $"<color=white>Player Grounded: {isGrounded}</color>\n";
        }
    }

    private void UpdatePlayerHealth()
    {
        if (playerHealth != null && healthData != null)
        {
            healthData.text = $"<color=red>Player Health: {playerHealth.GetCurrentHealth():F0}</color>\n";
        }
    }
}
