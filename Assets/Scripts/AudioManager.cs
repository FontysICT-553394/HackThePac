using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource pelletSource;
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioClip introClip;
    public AudioClip pelletClip1;
    public AudioClip pelletClip2;
    public AudioClip powerPelletClip;
    public AudioClip ghostEatenClip;
    public AudioClip deathClip;

    private bool alternatePelletSound = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayIntro()
    {
        sfxSource.PlayOneShot(introClip);
    }

    private float nextPelletTime = 0f;
    public float pelletCooldown = 0.08f;

    public void PlayPellet()
    {
        if (Time.time < nextPelletTime)
            return;

        pelletSource.pitch = Random.Range(0.95f, 1.05f);

        if (alternatePelletSound)
            pelletSource.PlayOneShot(pelletClip1);
        else
            pelletSource.PlayOneShot(pelletClip2);

        alternatePelletSound = !alternatePelletSound;
        nextPelletTime = Time.time + pelletCooldown;
    }

    public void PlayPowerPellet()
    {
        sfxSource.PlayOneShot(powerPelletClip);
    }

    public void PlayGhostEaten()
    {
        sfxSource.PlayOneShot(ghostEatenClip);
    }

    public void PlayDeath()
    {
        sfxSource.PlayOneShot(deathClip);
    }
}