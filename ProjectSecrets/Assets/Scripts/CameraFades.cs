using UnityEngine;

public class CameraFades : MonoBehaviour
{
    public Player player;
    public PortalPad portalPad;
    public Animator animator;
    public bool fading;

    public void FadeBlack()
    {
        animator.Play("FadeBlack");
        fading = true;
    }
    public void FadeWhite() 
    {
        animator.Play("FadeWhite");
        fading = true;
    }

    public void DoneFading()
    {
        fading = false;
    }

    public void Recover()
    {
        player.Recover();
    }

    public void Teleport()
    {
        portalPad.Teleport();
    }

}
