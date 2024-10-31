using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileFade : MonoBehaviour
{
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] BetterLerp.LerpType lerpType = BetterLerp.LerpType.Linear;
    private List<TilemapRenderer> _renderers;
    private List<Material> _fadeMaterials;

    private void Awake()
    {
        _renderers = new (GetComponentsInChildren <TilemapRenderer> ());
        // Assign materials
        _fadeMaterials = new();
        foreach (var t in _renderers)
        {
            _fadeMaterials.Add(t.material);
        }
    }

    public void FadeOutFloat(string propertyName)
    {
        StartCoroutine(Fader(propertyName, true));
    }

    public void FadeInFloat(string propertyName)
    {
        StartCoroutine(Fader(propertyName, false));
    }

    IEnumerator Fader(string propertyName, bool fadeOut = true)
    {
        // lerp the capacity
        var elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            // iterate the flash duration
            elapsedTime += Time.deltaTime;
            // lerp the flash amount
            var st = fadeOut ? 1f : 0f;
            var ed = fadeOut ? 0f : 1f;
            var currentAmount = BetterLerp.Lerp(st, ed, elapsedTime / fadeDuration, lerpType);
            SetFadeAmount(propertyName, currentAmount);
            yield return null;
        }
    }

    void SetFadeAmount(string propertyName, float amount)
    {
        // set Amount though the materials
        foreach (var t in _fadeMaterials)
        {
            t.SetFloat(propertyName, amount);
        }
    }
}
