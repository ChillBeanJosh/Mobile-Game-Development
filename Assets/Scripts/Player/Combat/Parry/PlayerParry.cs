using UnityEngine;

public struct ParryInput
{
    public bool ParryPressed;
}
public class PlayerParry : MonoBehaviour
{
    [Header("References: ")]
    [SerializeField] private PlayerMovement playerMovement;
    [Space]

    [Header("Parry Booleans: ")]
    [SerializeField] private bool _requestedParry;
    [SerializeField] private bool _isParrying;
    [SerializeField] private bool _parrySuccess;
    [Space]

    [Header("Parry Duration: ")]
    [SerializeField] private float parryDuration = 0.5f;
    [SerializeField] private float parryDurationTimer = 0f;
    [Space]

    [Header("Parry Cooldown: ")]
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private float parryCooldownTimer = 0f;
    [Space]

    [Header("Collider References: ")]
    [SerializeField] private GameObject parryHitBox;
    [SerializeField] private GameObject playerHurtBox;

    void Awake()
    {
        if (parryHitBox != null)
        {
            parryHitBox.SetActive(false);
        }
    }

    public void UpdateParry(ParryInput input)
    {
        _requestedParry |= input.ParryPressed;
    }

    public void ReadParry()
    {

        float dt = Time.fixedDeltaTime;
        float currentSpeed = playerMovement.GetCurrentVelocity().magnitude;

        //Decrement Parry CD Timer:
        if (parryCooldownTimer > 0f)
        {
            parryCooldownTimer -= dt;
        }

        if (_isParrying)
        {
            //Decrement Parry Duration Timer While Parrying is Active:
            parryDurationTimer -= dt;

            if (parryDurationTimer <= 0f)
            {
                EndParry();
            }
        }

        //Moving and Parry Input is Requested:
        if (currentSpeed > 0f && _requestedParry)
        {     
            Debug.Log("Parry executed!");

            //Parry Logic:
            if (parryCooldownTimer <= 0f)
            {
                StartParry();
            }
            _requestedParry = false; 
        }  
    }

    private void StartParry()
    {
        //Set Parry True and Reset Timers:
        _isParrying = true;
        parryDurationTimer = parryDuration;
        parryCooldownTimer = parryCooldown;

        //Enable Parry Hitbox, Disable Player Hurtbox:
        if (parryHitBox != null) parryHitBox.SetActive(true);
        if (playerHurtBox != null) playerHurtBox.SetActive(false);
    }

    private void EndParry()
    {
        //Set Parry False:
        _isParrying = false;

        //Disable Parry Hitbox, Enable Player Hurtbox:
        if (parryHitBox != null) parryHitBox.SetActive(false);
        if (playerHurtBox != null) playerHurtBox.SetActive(true);
    }

    //Used to Extend the Parry Duration Timer, for example when a successful parry is executed successfully:
    public void ExtendParryDuration(float duration)
    {
        parryDurationTimer += duration;
    }

    public bool IsParrying() => _isParrying;
    public bool SetSuccessfulParry(bool value) => _parrySuccess = value;
    public bool GetSuccessfulParry() => _parrySuccess;
}
