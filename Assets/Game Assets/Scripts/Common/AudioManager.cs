using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.15f;

    [Header("Sound Effects Library")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();

    [System.Serializable]
    public struct SoundEffect
    {
        public string name;
        public AudioClip clip;
    }

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure AudioSources are attached if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        InitializeSFXDictionary();
    }

    private void Start()
    {
        // Start background music automatically if configured
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void InitializeSFXDictionary()
    {
        sfxDictionary.Clear();
        foreach (var sfx in soundEffects)
        {
            if (sfx.clip != null && !string.IsNullOrEmpty(sfx.name))
            {
                if (!sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary.Add(sfx.name, sfx.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate Sound Effect name found: {sfx.name}");
                }
            }
        }
    }

    /// <summary>
    /// Plays background music. Loops automatically.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = defaultMusicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Plays a sound effect by its registered name in the library.
    /// </summary>
    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        if (sfxSource == null) return;

        if (sfxDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"Sound Effect '{soundName}' not found in AudioManager library!");
        }
    }

    /// <summary>
    /// Plays the default UI Click sound effect registered as "UiClick".
    /// </summary>
    public void PlayClickSound()
    {
        PlaySFX("UiClick");
    }

    /// <summary>
    /// Plays a sound effect directly by passing the AudioClip.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// Stops the currently playing background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Adjusts the volume of the background music.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        defaultMusicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = defaultMusicVolume;
        }
    }
}
