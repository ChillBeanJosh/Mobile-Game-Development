using UnityEngine;

public class ParryHitboxTrigger : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerParry playerParry;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem parrySuccessEffect;
    [SerializeField] private AudioSource audioSource;

    [Space]

    [Header("Parry Success Effects:")]
    [SerializeField] private float parryDurationExtension = 0.5f;
    [SerializeField] private float movementIncreaseFactor = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        //While Player Is Parring:
        if (playerParry != null && playerParry.IsParrying())
        {
            if(other.CompareTag("Parryable"))
            {
                Debug.Log($"[Parry Success!] Deflected or countered: {other.gameObject.name}");

                PlayParticleEffect();
                IncreaseSpeed();
                playerParry.SetSuccessfulParry(true);
                playerParry.ExtendParryDuration(parryDurationExtension);
            }
        }
    }

    private void PlayParticleEffect()
    {
        if (parrySuccessEffect != null)
        {
            parrySuccessEffect.Play();
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void IncreaseSpeed()
    {
        Vector3 currentVel = playerMovement.GetCurrentVelocity();
        Vector3 velocityGain = currentVel * movementIncreaseFactor;
        playerMovement.AddImpulse(velocityGain);
    }
}
