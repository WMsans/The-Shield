using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class GlobalVolumeManager : MonoBehaviour
{
    // Singleton
    public static GlobalVolumeManager Instance { get; private set; }
    private Volume _volume;
    private Bloom _bloom;
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private Tonemapping _tonemapping;
    private ColorAdjustments _colorAdjustments;
    private MotionBlur _motionBlur;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("GlobalVolumeManager already exists");
            Destroy(this);
            return;
        }
        _volume = GetComponent<Volume>();
        // Get properties
        _volume.profile.TryGet(out _bloom);
        _volume.profile.TryGet(out _vignette);
        _volume.profile.TryGet(out _chromaticAberration);
        _volume.profile.TryGet(out _tonemapping);
        _volume.profile.TryGet(out _colorAdjustments);
        _volume.profile.TryGet(out _motionBlur);
    }

    public void ChangeChromaticAberration(float start, float target, float duration, BetterLerp.LerpType type, bool invert = false)
    {
        StartCoroutine(SetChromaticAberration(start,target, duration, type, invert));
    }
    IEnumerator SetChromaticAberration(float start, float target, float duration, BetterLerp.LerpType type, bool invert = false)
    {
        var time = 0f;
        while (time < duration)
        {
            _chromaticAberration.intensity.value = BetterLerp.Lerp(start, target, time / duration, type, invert);
            time += Time.deltaTime;
            yield return null;
        }
        _chromaticAberration.intensity.value = (type == BetterLerp.LerpType.Pong ? start : target);
    }
}
