using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance = null;

    public AudioClip loginnusic;
    public AudioClip gameMusuc;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "LoginScene":
                if (audioSource.clip != loginnusic)
                {
                    PlayMusic(loginnusic, 0.3f);
                }
                break;

            case "ChoiceScene":
                if (audioSource.clip != loginnusic)
                {
                    PlayMusic(loginnusic, 0.3f);
                }
                break;

            case "GameScene":
                PlayMusic(gameMusuc, 0.2f);
                break;

            default:
                audioSource.Stop();
                break;
        }
    }
    void PlayMusic(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}