using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerParry playerParry;


    private void Update()
    {
        float speed = playerMovement.GetCurrentVelocity().magnitude;

        //Move Animation Speed Parameter Based on Player Movement Speed:
        animator.SetBool("isMoving", speed > 0.1f);

        //On Successful Parry, Trigger the Slash Animation:
        if (playerParry.GetSuccessfulParry())
        {
            //Fire Animation Trigger for Slash Animation:
            animator.SetTrigger("TriggerSlash");

            //Clear Parry Success Flag After Triggering Animation:
            playerParry.SetSuccessfulParry(false);
        }
    }
}


