using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance { get; private set; }
    [SerializeField] Transform follow;
    [SerializeField] float followSpeed = 0.3f;
    [SerializeField] float lookForward;
    [SerializeField] Vector2 frameInDistance;

    private Vector3 _moveDamp;
    private Vector3 _lastFollowPos;
    public Camera Cam {get; private set;}
    private float _camZ;
    private Vector2 _minPoint;
    private Vector2 _maxPoint;
    private bool _limitinBuff;
    public CameraLimiter CameraLimiter { get; set; }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.LogError("Found more than one Virtual Camera in the scene.");
        }
        else
        {
            Instance = this;
        }
        Cam = GetComponent<Camera>();
    }
    void Start()
    {
        _lastFollowPos = follow.position;
        _camZ = Cam.transform.position.z;
        CameraLimiter = null;
    }
    void Update()
    {
        // Find target pos
        var targetPos = follow.position + (follow.position - _lastFollowPos) * lookForward;
        if (Vector2.Distance(_minPoint, _maxPoint) > 0f && CameraLimiter != null)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, _minPoint.x + Extents(Cam).x, _maxPoint.x - Extents(Cam).x);
            targetPos.y = Mathf.Clamp(targetPos.y, _minPoint.y + Extents(Cam).y, _maxPoint.y - Extents(Cam).y);
        }
        // Move camera toward target
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _moveDamp, followSpeed);
        // Frame in
        if (!(Mathf.Abs(targetPos.x - transform.position.x) > frameInDistance.x ||
              Mathf.Abs(targetPos.y - transform.position.y) > frameInDistance.y))
        {
            _limitinBuff = false;
        }
        if (Mathf.Abs(targetPos.x - transform.position.x) > frameInDistance.x && !_limitinBuff)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos,
                Mathf.Abs(targetPos.x - transform.position.x) - frameInDistance.x);
        }
        if (Mathf.Abs(targetPos.y - transform.position.y) > frameInDistance.y && !_limitinBuff)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos,
                Mathf.Abs(targetPos.y - transform.position.y) - frameInDistance.y);
        }
        // Reset target
        _lastFollowPos = follow.position;
        // Set camera Z position
        Cam.transform.position = new(transform.position.x, transform.position.y, _camZ);
    }

    public void Limitin(Vector2 minPoint, Vector2 maxPoint)
    {
        _minPoint = minPoint;
        _maxPoint = maxPoint;
        _limitinBuff = true;
    }

    public void Limitout()
    {
        print("OUOUOUOUOUOT");
        _limitinBuff = true;
    }
    private Vector2 Extents(Camera cam)
    {
        if (cam.orthographic)
            return new(cam.orthographicSize * Screen.width / Screen.height, cam.orthographicSize);
        else
        {
            Debug.LogError("Camera is not orthographic!", cam);
            return new();
        }
    }
    private Vector2 BoundsMin(Camera cam)
    {
        Vector2 pos = cam.transform.position;
        return pos - Extents(cam);
    }
    private Vector2 BoundsMax(Camera cam)
    {
        Vector2 pos = cam.transform.position;
        return pos + Extents(cam);
    }
}
