using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerManager : MonoBehaviour
{
    // Singleton
    public static AudioPlayerManager Instance { get; private set; }
    [SerializeField] private AudioSource _soundFxSource;
    [SerializeField] private AudioSource _musicSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("There is more than one AudioPlayerManager in the scene");
            Destroy(this);
            return;
        }
    }

    public void PlayAudio(List<AudioClip> clips, Vector3 source, float volume, float pitch, float delay, bool soundFx = true)
    {
        var audioSource = Instantiate(soundFx ? _soundFxSource : _musicSource, source, Quaternion.identity);
        var clip = clips[Random.Range(0, clips.Count)];
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayDelayed(delay);
        Destroy(audioSource.gameObject, audioSource.clip.length + delay);   
    }
    public void PlayAudio(AudioClip clip, Transform source, float volume, bool soundFx = true) => PlayAudio(new List<AudioClip> { clip }, source.position, volume, 1f, 0f, soundFx);
    public void PlayAudio(List<AudioClip> clips, Transform source, float volume, bool soundFx = true) => PlayAudio(clips, source.position, volume, 1f, 0f, soundFx);
    
}
