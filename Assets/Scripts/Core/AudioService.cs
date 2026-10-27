using UnityEngine;

public sealed class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    private AudioSource source;

    public static AudioService Ensure()
    {
        if (Instance != null) return Instance;
        GameObject host = new GameObject("AudioService");
        return host.AddComponent<AudioService>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        AudioListener.volume = SaveService.MasterVolume;
    }

    public void Play(string cue)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{cue}");
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip);
        }
    }
}

