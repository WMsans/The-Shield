using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimiter : MonoBehaviour
{
    [SerializeField] Collider2D collisionBound;
    [SerializeField] Collider2D cameraBound;

    private CameraFollower _cameraFollower;
    private bool _enabled;
    void Awake()
    {
        _enabled = false;

        GetComponent<SpriteRenderer>().enabled = false;
    }
    private void Start()
    {
        _cameraFollower = CameraFollower.Instance;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _enabled = true;
            if (cameraBound.bounds.Contains(collision.gameObject.transform.position)){
                _cameraFollower.CameraLimiter = this;
                //Make this the limiter
                _cameraFollower.MinPoint = cameraBound.bounds.min;
                _cameraFollower.MaxPoint = cameraBound.bounds.max;
            }
            else
            {
                if (_cameraFollower.CameraLimiter == this)
                {
                    _cameraFollower.CameraLimiter = null;
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && _enabled)
        {
            _enabled = false;
            if (_cameraFollower.CameraLimiter == this)
            {
                _cameraFollower.CameraLimiter = null;
            }
        }
    }
}
