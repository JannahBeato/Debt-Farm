using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float defaultVolume = 0.5f;
    [SerializeField] private bool playOnStart = true;

    private const string MusicVolumeKey = "MusicVolume";

    public float CurrentVolume { get; private set; } = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        CurrentVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        ApplyVolume();

        if (backgroundMusic != null)
            audioSource.clip = backgroundMusic;
    }

    private void Start()
    {
        if (playOnStart && backgroundMusic != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        CurrentVolume = Mathf.Clamp01(volume);
        ApplyVolume();

        PlayerPrefs.SetFloat(MusicVolumeKey, CurrentVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume()
    {
        if (audioSource != null)
            audioSource.volume = CurrentVolume;
    }
}