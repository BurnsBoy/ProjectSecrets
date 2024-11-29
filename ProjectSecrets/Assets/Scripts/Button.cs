using UnityEngine;

public class Button : MonoBehaviour
{
    public MovingPlatformSystem movingPlatformSystem;
    public Animator animator;
    public AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            if (!movingPlatformSystem.active)
            {
                movingPlatformSystem.meshRenderer.material = movingPlatformSystem.onMaterial;
                audioSource.Play();
                movingPlatformSystem.active = true;
                animator.SetBool("pressed", true);
            }
        }
    }
}
