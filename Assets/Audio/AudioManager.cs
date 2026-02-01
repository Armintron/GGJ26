using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioSource> sfxSources = new List<AudioSource>();

    [Header("Settings")]
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

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

        // Auto-setup sources if not assigned
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSources.Count == 0) sfxSources.Add(gameObject.AddComponent<AudioSource>());
        
        musicSource.loop = true;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSFXSource();
        source.PlayOneShot(clip);
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying) return source;
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        sfxSources.Add(newSource);
        return newSource;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
