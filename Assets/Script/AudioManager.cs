using UnityEngine;
public enum SoundType
{
    Win,
    Draw,
    CellClick
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip winSound;
    public AudioClip drawSound;
    public AudioClip cellClick;

    private AudioSource audioSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(SoundType type)
    {
        AudioClip clip = null;

        switch (type)
        {
            case SoundType.Win:
                clip = winSound;
                break;

            case SoundType.Draw:
                clip = drawSound;
                break;

            case SoundType.CellClick:
                clip = cellClick;
                break;
        }

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}

