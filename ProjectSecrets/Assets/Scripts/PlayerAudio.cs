using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip footStep;
    public AudioClip jump;
    public AudioSource jetpack;
    public AudioClip jetpackThrust;
    public AudioClip jetpackEmpty;
    public AudioClip grappleShoot;
    public AudioSource grapplePull;
    public AudioClip magnetStep;
    public AudioClip lavaSizzle;
    public AudioClip mudSink;
    public AudioSource darkDamage;
    public AudioClip lostLife;
    public AudioClip padClick;
    public AudioClip teleport;
    public AudioClip toolPickup;
    public bool onMagSurface;


    void RandomizePitchAndVolume(float pitchRange, float volumeRange)
    {
        audioSource.pitch = Random.Range(1 - pitchRange, 1 + pitchRange);
        audioSource.volume = Random.Range(1 - volumeRange, 1 + volumeRange);
    }
    void FadeAudio(bool on, AudioSource audio, float speed)
    {
        if (on && audio.volume < 1)
        {
            audio.volume += Time.deltaTime * speed;
        }
        else if (!on && audio.volume > 0)
        {
            audio.volume -= Time.deltaTime * speed;
        }
    }
    public void Footstep()
    {
        RandomizePitchAndVolume(.1f, .25f);
        audioSource.PlayOneShot(!onMagSurface ? footStep : magnetStep);
    }
    public void Land()
    {
        RandomizePitchAndVolume(.1f, .25f);
        audioSource.volume += .5f;
        audioSource.PlayOneShot(footStep);
    }
    public void Jump()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(jump);
    }
    public void Jetpack(bool on)
    {
        FadeAudio(on, jetpack, 5);
    }
    public void JetpackThrust()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(jetpackThrust);
    }
    public void JetpackEmpty()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(jetpackEmpty);
    }
    public void GrappleShoot()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(grappleShoot);
    }
    public void GrapplePull(bool on)
    {
        FadeAudio(on, grapplePull, 20);
    }
    public void MagnetStep()
    {
        RandomizePitchAndVolume(.1f, .25f);
        audioSource.PlayOneShot(magnetStep);
    }
    public void LavaSizzle()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(lavaSizzle);
    }
    public void MudSink()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(mudSink);
    }
    public void DarkDamage(bool on)
    {
        FadeAudio(on, darkDamage, 5);
    }
    public void LostLife()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(lostLife);
    }
    public void PadClick()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(padClick);
    }
    public void Teleport()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(teleport);
    }
    public void HealthPickup()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(toolPickup);
    }
    public void PuzzlePickup()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(toolPickup);
    }
    public void ToolPickup()
    {
        RandomizePitchAndVolume(.1f, 0);
        audioSource.PlayOneShot(toolPickup);
    }
}
