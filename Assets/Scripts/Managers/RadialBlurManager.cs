using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialBlurManager : MaterialManager
{
    // Singleton
    public static RadialBlurManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one RadialBlurManager in the scene");
            Destroy(this);
            return;
        }
    }
    public override void UpdateMaterial(float start, float end, float duration, BetterLerp.LerpType type, bool invert = false)
    {
        StartCoroutine(SetRadialBlur(start, end, duration, type, invert));
    }

    public void SetCenter(Vector2 center)
    {
        material.SetVector("_Center", center);
    }
    IEnumerator SetRadialBlur(float start, float end, float duration, BetterLerp.LerpType type, bool invert = false)
    {
        var time = 0f;
        while (time < duration)
        {
            material.SetFloat("_Intensity", BetterLerp.Lerp(start, end, time / duration, type, invert));
            time += Time.deltaTime;
            yield return null;
        }
        material.SetFloat("_Intensity", type == BetterLerp.LerpType.Pong ? start : end);
    }
}
