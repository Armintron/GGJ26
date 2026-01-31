using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Integration")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

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
        
        // Assign mixer groups
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;
        foreach (var source in sfxSources)
        {
            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
        }
        
        musicSource.loop = true;
    }

    public void Update()
    {
        UpdateMixerVolumes();
    }

    private void UpdateMixerVolumes()
    {
        if (mixer == null) return;

        // Convert linear 0-1 to Decibels
        mixer.SetFloat(musicVolumeParam, Mathf.Log10(Mathf.Max(0.0001f, musicVolume)) * 20f);
        mixer.SetFloat(sfxVolumeParam, Mathf.Log10(Mathf.Max(0.0001f, sfxVolume)) * 20f);
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
        if (sfxGroup != null) newSource.outputAudioMixerGroup = sfxGroup;
        sfxSources.Add(newSource);
        return newSource;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
