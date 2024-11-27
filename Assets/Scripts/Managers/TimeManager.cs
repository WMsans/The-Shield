using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    private readonly float _defaultTimeScale = 1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one instance of TimeManager");
            Destroy(this);
        }
    }

    public void SetTimeScale(float scale, float duration)
    {
        StartCoroutine(SetTimeScaleCoroutine(duration, scale));
    }
    private IEnumerator SetTimeScaleCoroutine(float duration, float scale)
    {
        Time.timeScale = scale;
        yield return new WaitForSeconds(duration);
        ResetTimeScale();
    }

    private void ResetTimeScale()
    {
        Time.timeScale = _defaultTimeScale;
    }

    public void FrozenTime(float duration, float frScale, float toScale)
    {
        Time.timeScale = frScale;
        StartCoroutine(FrozenTimeCoroutine(duration, frScale, toScale));
    }
    public void FrozenTime(float duration, float frScale) => FrozenTime(duration, frScale, frScale);
    private IEnumerator FrozenTimeCoroutine(float duration, float frScale, float toScale)
    {
        var timer = duration;
        while (timer > 0f)
        {
            Time.timeScale = BetterLerp.Lerp(frScale, toScale, timer / duration, BetterLerp.LerpType.Sin);
            timer -= Time.deltaTime;
            yield return null;
        }
        ResetTimeScale();
    }
}
