using UnityEngine;
 
[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(Vector2 deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;
 
    private Vector2 _oldPosition;
 
    void Start()
    {
        _oldPosition = transform.position;
    }
 
    void Update()
    {
        UpdateCamera();
    }
    void UpdateCamera()
    {
        if (Vector2.Distance(_oldPosition, transform.position) > 0.1f)
        {
            if (onCameraTranslate != null)
            {
                var delta = _oldPosition - (Vector2)transform.position;
                onCameraTranslate(delta);
            }
 
            _oldPosition = transform.position;
        }
    }
}