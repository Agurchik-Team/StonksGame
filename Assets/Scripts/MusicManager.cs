using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Музыка")]
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private float musicVolume = 0.15f;

    private AudioSource audioSource;

    private void Awake()
    {
        // НЕ ИСПОЛЬЗУЕМ DontDestroyOnLoad — чтобы при рестарте всё создавалось заново
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.loop = true;
        audioSource.volume = musicVolume;
    }

    private void Start()
    {
        // Запускаем фоновую музыку
        PlayGameMusic();
    }

    /// <summary>
    /// Фоновая музыка игры
    /// </summary>
    public void PlayGameMusic()
    {
        if (gameMusic != null && audioSource != null)
        {
            audioSource.clip = gameMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// РЕЗКАЯ остановка и включение музыки поражения
    /// </summary>
    public void PlayGameOverMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // ← МГНОВЕННАЯ ОСТАНОВКА
            
            if (gameOverMusic != null)
            {
                audioSource.clip = gameOverMusic;
                audioSource.loop = false; // Один раз
                audioSource.Play();
            }
        }
    }
}