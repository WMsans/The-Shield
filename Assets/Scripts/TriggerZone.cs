using UnityEngine;
using UnityEngine.Events;
 
public class TriggerZone : MonoBehaviour
{
    [SerializeField] bool oneShot;
    private bool _alreadyEntered;
    private bool _alreadyExited;
 
    [SerializeField] string collisionTag;
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_alreadyEntered)
            return;
 
        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;
 
        onTriggerEnter?.Invoke();
 
        if (oneShot)
            _alreadyEntered = true;
    }
 
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_alreadyExited)
            return;
 
        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;
 
        onTriggerExit?.Invoke();
 
        if (oneShot)
            _alreadyExited = true;
    }
}