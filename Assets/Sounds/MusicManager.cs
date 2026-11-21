using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public struct SceneMusicEntry
{
    public string sceneName;
    public AudioClip musicClip;
    public bool isMenuScene;
}

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Música de Menús")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private List<string> menuSceneNames = new List<string> { "MainMenu", "OptionsMenu", "LevelSelect" };

    [Header("Música por Escena / Nivel")]
    [SerializeField] private AudioClip defaultLevelMusic;
    [SerializeField] private List<SceneMusicEntry> sceneMusicOverrides = new List<SceneMusicEntry>();

    [Header("Ajustes de Reproducción")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    public AudioSource audioSource;
    public float SfxVolume => sfxVolume;
    private bool wasInMenuScene = false;

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
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Evaluar la escena inicial si ya estaba cargada antes de suscribirse
        EvaluateSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateSceneMusic(scene.name);
    }

    public void SetVolume(float newVolume)
    {
        musicVolume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float newSfxVolume)
    {
        sfxVolume = Mathf.Clamp01(newSfxVolume);
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || audioSource == null) return;
        float finalScale = Mathf.Clamp01(volumeScale) * sfxVolume;
        if (finalScale > 0f)
        {
            audioSource.PlayOneShot(clip, finalScale);
        }
    }

    private void EvaluateSceneMusic(string sceneName)
    {
        // Buscar si existe una configuración específica para esta escena
        SceneMusicEntry? specificEntry = null;
        foreach (var entry in sceneMusicOverrides)
        {
            if (entry.sceneName == sceneName)
            {
                specificEntry = entry;
                break;
            }
        }

        bool isMenu = specificEntry.HasValue
            ? specificEntry.Value.isMenuScene
            : menuSceneNames.Contains(sceneName);

        if (isMenu)
        {
            AudioClip targetMenuClip = specificEntry.HasValue && specificEntry.Value.musicClip != null
                ? specificEntry.Value.musicClip
                : menuMusic;

            // Si ya estábamos en un menú y ya suena el mismo clip, no se corta ni reinicia
            if (wasInMenuScene && audioSource.clip == targetMenuClip && audioSource.isPlaying)
            {
                return;
            }

            // Si venimos de un nivel (o arrancamos), tocamos la canción del menú desde el principio
            audioSource.volume = musicVolume;
            PlayClipFromStart(targetMenuClip);
            wasInMenuScene = true;
        }
        else
        {
            wasInMenuScene = false;

            AudioClip targetLevelClip = specificEntry.HasValue && specificEntry.Value.musicClip != null
                ? specificEntry.Value.musicClip
                : defaultLevelMusic;
            audioSource.volume = musicVolume;
            PlayClipFromStart(targetLevelClip);
        }
    }

    private void PlayClipFromStart(AudioClip clip)
    {
        if (clip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.time = 0f;
        audioSource.Play();
    }
}