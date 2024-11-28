using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFadein : MonoBehaviour
{
    [SerializeField] private float fadeTime = .2f;
    [SerializeField] private float startIntensity = 0f;
    [SerializeField] private float endIntensity = .3f;
    [SerializeField] private AnimationCurve fadeCurve;
    private Light2D _lights;
    private float _time;
    private void Awake()
    {
        _lights = GetComponent<Light2D>();
    }
    private void Start()
    {
        SetIntensity(startIntensity);
        Debug.Log("Light Fadein!" + startIntensity);
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if(_time < fadeTime)
        {
            SetIntensity(Mathf.Lerp(startIntensity, endIntensity, fadeCurve.Evaluate(_time / fadeTime)));
        }
    }

    private void SetIntensity(float intensity)
    {
        _lights.intensity = intensity;
    }
}
