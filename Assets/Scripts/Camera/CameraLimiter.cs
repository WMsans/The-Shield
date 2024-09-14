using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimiter : MonoBehaviour
{
    [SerializeField] Collider2D collisionBound;
    [SerializeField] Collider2D cameraBound;

    CameraFollower cameraFollower;
    bool _enabled = false;
    void Awake()
    {
        _enabled = false;

        GetComponent<SpriteRenderer>().enabled = false;
    }
    private void Start()
    {
        cameraFollower = CameraFollower.Instance;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _enabled = true;
            if (cameraBound.bounds.Contains(collision.gameObject.transform.position)){
                cameraFollower.CameraLimiter = this;
                //Make this the limiter
                cameraFollower.MinPoint = cameraBound.bounds.min;
                cameraFollower.MaxPoint = cameraBound.bounds.max;
            }
            else
            {
                if (cameraFollower.CameraLimiter == this)
                {
                    cameraFollower.CameraLimiter = null;
                }
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && _enabled)
        {
            _enabled = false;
            if (cameraFollower.CameraLimiter == this)
            {
                cameraFollower.CameraLimiter = null;
            }
        }
    }
}
