using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageFlash : MonoBehaviour
{
    private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
    [ColorUsage(true, true)] [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] BetterLerp.LerpType lerpType = BetterLerp.LerpType.Linear;
    private List<SpriteRenderer> _spr;
    private List<Material> _flashMat;

    private void Awake()
    {
        _spr = new(GetComponentsInChildren<SpriteRenderer>());
        // Assign materials
        _flashMat = new();
        for (int i = 0; i < _spr.Count; i++)
        {
            _flashMat.Add(_spr[i].material);
        }
    }

    private Coroutine _flashCoroutine;
    public void Flash()
    {
        _flashCoroutine = StartCoroutine(Flasher());
    }

    IEnumerator Flasher()
    {
        // set the color
        SetFlashColor(flashColor);
        // lerp the capacity
        var currentAmount = 0f;
        var elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            // iterate the flash duration
            elapsedTime += Time.deltaTime;
            // lerp the flash amount
            currentAmount = BetterLerp.Lerp(1f, 0f, elapsedTime / flashDuration, lerpType);
            SetFlashAmount(currentAmount);
            yield return null;
        }
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
}
