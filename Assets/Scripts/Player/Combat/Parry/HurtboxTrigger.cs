using UnityEngine;

public class HurtboxTrigger : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem playerHurtEffect;
    [SerializeField] private AudioSource audioSource;

    [Space]

    [Header("Hurtbox Effects:")]
    [SerializeField] private float movementReductionFactor = 0.5f; 

    private void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag("Parryable"))
        {
            Debug.Log($"[Hurtbox Triggered] Player hit by: {other.gameObject.name}");

            PlayParticleEffect(); 
            ReduceSpeed();
        }
    }

    private void PlayParticleEffect()
    {
       if(playerHurtEffect != null)
       { 
           playerHurtEffect.Play();
       }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void ReduceSpeed()
    {
        Vector3 currentVel = playerMovement.GetCurrentVelocity();
        Vector3 velocityLoss = -currentVel * movementReductionFactor;
        playerMovement.AddImpulse(velocityLoss);
    }
}
