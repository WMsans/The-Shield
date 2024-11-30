using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    // Singleton
    public static SoundMixerManager Instance { get; private set; }
    [SerializeField] private AudioMixer _audioMixer;
    [Header("Debug Volume")]
    [Range(0.0001f, 1f)] [SerializeField] private float _masterVolume;
    [Range(0.0001f, 1f)][SerializeField] private float _musicVolume;
    [Range(0.0001f, 1f)][SerializeField] private float _soundFxVolume;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one SoundMixerManager in the scene");
            Destroy(this);
            return;
        }
    }
    void Update()
    {
        SetMasterVolume(_masterVolume);
        SetMusicVolume(_musicVolume);
        SetSoundVolume(_soundFxVolume);
    }
    public void SetMasterVolume(float volume)
    {
        _audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    public void SetMusicVolume(float volume)
    {
        _audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }
    public void SetSoundVolume(float volume)
    {
        _audioMixer.SetFloat("SoundFxVolume", Mathf.Log10(volume) * 20f);
    }
}
