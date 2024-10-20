using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageFlash : MonoBehaviour
{
    private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    [ColorUsage(true, true)] [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] BetterLerp.LerpType lerpType = BetterLerp.LerpType.Linear;
    PlayerController _player;
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

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    public void Flash()
    {
        StartCoroutine(Flasher());
    }
    IEnumerator Flasher()
    {
        // set the color
        SetFlashColor(flashColor);
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

        InvincibleFlash();
    }
    public void InvincibleFlash()
    {
        StartCoroutine(InvincibleFlasher());
    }
    IEnumerator InvincibleFlasher()
    {
        var elapsed = 0f;
        var next = elapsed + .2f;
        var trans = false;
        //var normalColor = _flashMat[0].color;
        while (_player.Invincible)
        {
            elapsed += Time.deltaTime;
            if (elapsed > next)
            {
                next = elapsed + .2f;
                SetFlashAlpha(trans ? 1f : 0f);
                trans = !trans;
            }
            yield return null;
        }
        SetFlashAlpha(1f);
    }
    void SetFlashColor(Color color)
    {
        // set color though the materials
        foreach (var t in _flashMat)
        {
            t.SetColor(FlashColor, color);
        }
    }

    void SetFlashAmount(float amount)
    {
        // set Amount though the materials
        foreach (var t in _flashMat)
        {
            t.SetFloat(FlashAmount, amount);
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
