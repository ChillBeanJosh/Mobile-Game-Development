using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class JoystickActivation : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private OnScreenStick JoystickControl;
    [SerializeField] private OnScreenButton Impulse;
    [SerializeField] private Image JoystickImage;

    [Header("Settings: ")]
    [SerializeField] private float fadeDuration = 0.5f;
    private float currentFadeTime;

    public void Update()
    {
        float currentSpeed = playerMovement.GetCurrentVelocity().magnitude;

        if (currentSpeed > 0f)
        {
            Impulse.gameObject.SetActive(false);

            //Increment Fade Timer:
            currentFadeTime += Time.deltaTime;

            if (currentFadeTime >= fadeDuration)
            {
                JoystickControl.gameObject.SetActive(false);
                JoystickImage.enabled = false;
            };
        }
        else
        {
            Impulse.gameObject.SetActive(true);
            JoystickControl.gameObject.SetActive(true);
            JoystickImage.enabled = true;
        }
    }
}