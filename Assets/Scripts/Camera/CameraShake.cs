using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Singleton pattern
    public static CameraShake Instance { get; private set; }
    [SerializeField] Transform cameraTransform;
    private float _shakeAmount;
    private float _decreaseFactor;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of CameraShake found.");
            Destroy(this);
        }
    }

    public void ShakeCamera(float amount, float duration)
    {
        _shakeAmount = amount;
        _decreaseFactor = amount / duration;
    }
    public void ShakeCamera(float amount) => ShakeCamera(amount, 0.5f);
    void Update()
    {
        if (_shakeAmount > 0.0f)
        {
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x + Random.Range(-1.0f, 1.0f) * _shakeAmount,
                cameraTransform.localPosition.y + Random.Range(-1.0f, 1.0f) * _shakeAmount,
                cameraTransform.localPosition.z
            );
            _shakeAmount -= Time.deltaTime * _decreaseFactor;
        }
    }
}
