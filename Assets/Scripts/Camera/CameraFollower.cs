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

    Vector3 _moveDamp = new();
    Vector3 _lastFollowPos = new();
    public Camera Cam {get; private set;}
    float camZ;
    public Vector2 MinPoint { get; set; } = Vector2.zero;
    public Vector2 MaxPoint { get; set; } = Vector2.zero;
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
        camZ = Cam.transform.position.z;
        CameraLimiter = null;
    }
    void Update()
    {
        // Find target pos
        var targetPos = follow.position + (follow.position - _lastFollowPos) * lookForward;
        if (Vector2.Distance(MinPoint, MaxPoint) > 0f && CameraLimiter != null)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, MinPoint.x + Extents(Cam).x, MaxPoint.x - Extents(Cam).x);
            targetPos.y = Mathf.Clamp(targetPos.y, MinPoint.y + Extents(Cam).y, MaxPoint.y - Extents(Cam).y);
        }
        // Move camera toward target
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _moveDamp, followSpeed);
        if (Mathf.Abs(targetPos.x - transform.position.x) > frameInDistance.x)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos,
                Mathf.Abs(targetPos.x - transform.position.x) - frameInDistance.x);
        }
        if (Mathf.Abs(targetPos.y - transform.position.y) > frameInDistance.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos,
                Mathf.Abs(targetPos.y - transform.position.y) - frameInDistance.y);
        }
        // Reset target
        _lastFollowPos = follow.position;
        // Set camera Z position
        Cam.transform.position = new(transform.position.x, transform.position.y, camZ);
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
