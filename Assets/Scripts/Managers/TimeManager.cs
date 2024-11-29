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

    public void SetTimeScale(float duration, float scale)
    {
        StartCoroutine(SetTimeScaleCoroutine(duration, scale));
    }
    private IEnumerator SetTimeScaleCoroutine(float duration, float scale)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        ResetTimeScale();
    }

    private void ResetTimeScale()
    {
        Time.timeScale = _defaultTimeScale;
    }

    public void FrozenTime(float duration, float scale) => SetTimeScale(duration, scale);
}
