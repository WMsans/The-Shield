using UnityEngine;
using UnityEngine.Events;
 
public class TriggerZone : MonoBehaviour, IPersistant
{
    [SerializeField] bool oneShot;
    [SerializeField] bool persistant;
    private bool _alreadyEntered;
    private bool _alreadyExited;
 
    [SerializeField] string collisionTag;
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
    [SerializeField]private string _id;

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

    public bool Initialized { get; set; }
    public void OnInitialize()
    {
        if(persistant) return;
        _alreadyEntered = false;
        _alreadyExited = false;
    }
    public void OnReset()
    {
        if(persistant) return;
        _alreadyEntered = false;
        _alreadyExited = false;
    }

    string IPersistant.Id
    {
        get => _id;
        set => _id = value;
    }

    public void SaveData()
    {
        if(!persistant) return;
        ES3.Save(_id + "AlreadyEntered", _alreadyEntered);
        ES3.Save(_id + "AlreadyExited", _alreadyExited);
    }

    public void LoadData()
    {
        if(!persistant) return;
        _alreadyEntered = ES3.Load(_id + "AlreadyEntered", _alreadyEntered);
        _alreadyExited = ES3.Load(_id + "AlreadyExited", _alreadyExited);
    }
    [ContextMenu("Generate Guid")]
    public void GenerateGuid()
    {
        IPersistant.GenerateGuids(ref _id);
    }
}