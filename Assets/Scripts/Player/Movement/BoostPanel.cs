using UnityEngine;

public class BoostPanel : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem boostEffect;
    [Space]

    [Header("Boost Panel Settings:")]
    [SerializeField] private float boostAcceleration = 1000f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Boost Panel Activated!] Boosting player: {other.gameObject.name}");
            ApplyBoost();
            PlayBoostEffect();
        }
    }

    private void ApplyBoost()
    {
        if (playerMovement != null)
        {
            Vector3 boostVector = transform.forward * boostAcceleration;
            playerMovement.AddImpulse(boostVector);
        }
    }

    private void PlayBoostEffect()
    {
        if (boostEffect != null)
        {
            boostEffect.Play();
        }
    }
}
