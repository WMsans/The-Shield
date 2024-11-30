using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> clips;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool soundFx = true;
    #if soundFx == false
    [SerializeField] private bool playOnAwake = false;
    #endif

    public void PlayAudio()
    {
        AudioPlayerManager.Instance?.PlayAudio(clips, transform, volume, soundFx);
    }

    public void PlayAudioWithIndex(int index)
    {
        AudioPlayerManager.Instance?.PlayAudio(clips[index], transform, volume, soundFx);
    }
    void Start()
    {
        if (playOnAwake)
        {
            PlayAudio();
        }
    }
}
