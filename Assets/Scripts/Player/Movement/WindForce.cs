using UnityEngine;

public class WindForce : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem windEffect;
    [Space]

    [Header("Wind Force Settings:")]
    [SerializeField] private Vector3 windDirection;
    [SerializeField] private float windStrength = 500f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Wind Force Activated!] Applying wind force to player: {other.gameObject.name}");
            ApplyWindForce();
            PlayWindEffect();
        }
    }

    private void ApplyWindForce()
    {
        if (playerMovement != null)
        {
            Vector3 windForceVector = windDirection.normalized * windStrength;
            playerMovement.AddImpulse(windForceVector);
        }
    }

    private void PlayWindEffect()
    {
        if (windEffect != null)
        {
            windEffect.Play();
        }
    }
}
