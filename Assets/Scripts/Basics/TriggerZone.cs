using UnityEngine;
using UnityEngine.Events;
 
public class TriggerZone : MonoBehaviour, IPersistant
{
    [SerializeField] bool oneShot;
    [SerializeField] bool persistant;
    private bool _alreadyEntered;
 
    [SerializeField] string collisionTag;
    [SerializeField] bool acceptTriggerCollider;
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
    [SerializeField]private string _id;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_alreadyEntered)
            return;
        if(!acceptTriggerCollider && collision.isTrigger)
            return;
 
        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;
 
        onTriggerEnter?.Invoke();
 
        if (oneShot)
            _alreadyEntered = true;
    }
 
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_alreadyEntered)
            return;
        
        if(!acceptTriggerCollider && collision.isTrigger)
            return;
        
        if (!string.IsNullOrEmpty(collisionTag) && !collision.CompareTag(collisionTag))
            return;
 
        onTriggerExit?.Invoke();
    }

    public bool Initialized { get; set; }
    public void OnInitialize()
    {
        if(persistant) return;
        _alreadyEntered = false;
    }
    public void OnReset()
    {
        if(persistant) return;
        _alreadyEntered = false;
    }

    public void SaveData()
    {
        if(!persistant) return;
        ES3.Save(_id + "AlreadyEntered", _alreadyEntered);
    }

    public void LoadData()
    {
        if(!persistant) return;
        _alreadyEntered = ES3.Load(_id + "AlreadyEntered", _alreadyEntered);
    }
    [ContextMenu("Generate Guid")]
    public void GenerateGuid()
    {
        IPersistant.GenerateGuids(ref _id);
    }
}