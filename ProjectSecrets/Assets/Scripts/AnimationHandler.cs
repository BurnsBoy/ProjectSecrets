using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    InputSystem_Actions controls;
    public Animator animator;
    public Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = new InputSystem_Actions();
        controls.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        float moveSpeed = controls.Player.Move.ReadValue<Vector2>().magnitude;
        if (!player.grappling && player.isGrounded)
        {
            if (player.controls.Player.enabled && moveSpeed > 0 && !player.cameraFades.fading)
            {
                animator.SetBool("Moving", true);
                animator.speed = moveSpeed;
            }
            else
            {
                animator.SetBool("Moving", false);
                animator.speed = 1;
            }
        }
        else
        {
            animator.speed = 1;
        }
        animator.SetBool("Grappling", player.grappling);
        animator.SetBool("Airborne", !player.isGrounded);
        animator.SetBool("JetPack", player.useJetpack && player.jetpackCharge > 0);
    }
}
