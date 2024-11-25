using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using AllIn1SpriteShader;

public class DamageFlash : MonoBehaviour
{
    private static readonly int FlashAmount = Shader.PropertyToID("_HitEffectBlend");
    private static readonly int Alpha = Shader.PropertyToID("_GhostBlend");
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.25f;
    [SerializeField] BetterLerp.LerpType lerpType = BetterLerp.LerpType.Linear;
    private List<SpriteRenderer> _spr;
    private List<Material> _flashMat;

    private void Awake()
    {
        _spr = new(GetComponentsInChildren<SpriteRenderer>());
        
        // Assign materials
        _flashMat = new();
        foreach (var t in _spr)
        {
            _flashMat.Add(t.material);
        }
    }

    public void Flash(float duration)
    {
        StartCoroutine(Flasher(duration));
    }
    IEnumerator Flasher(float duration)
    {
        // lerp the capacity
        var elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            // iterate the flash duration
            elapsedTime += Time.deltaTime;
            // lerp the flash amount
            var currentAmount = BetterLerp.Lerp(1f, 0f, elapsedTime / flashDuration, lerpType);
            SetFlashAmount(currentAmount);
            yield return null;
        }

        InvincibleFlash(duration);
    }
    public void InvincibleFlash(float duration)
    {
        StartCoroutine(InvincibleFlasher(duration));
    }
    IEnumerator InvincibleFlasher(float duration)
    {
        var flashPeriod = .2f;
        var elapsed = 0f;
        var next = elapsed + flashPeriod;
        var trans = false;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (elapsed > next)
            {
                next = elapsed + flashPeriod;
                trans = !trans;
            }
            SetFlashAlpha(BetterLerp.Lerp(trans ? 1f : 0f, trans ? 0f : 1f, (next - elapsed) / flashPeriod, lerpType));
            yield return null;
        }
        SetFlashAlpha(0f);
    }

    void SetFlashAmount(float amount)
    {
        // set Amount though the materials
        foreach (var t in _flashMat)
        {
            t.SetFloat(FlashAmount, amount);
            t.SetColor("_HitEffectColor", flashColor);
        }
    }

    void SetFlashAlpha(float alpha)
    {
        foreach (var t in _flashMat)
        {
            t.SetFloat(Alpha, alpha);
        }
    }
}
