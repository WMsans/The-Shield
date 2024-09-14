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

    Vector3 _moveDamp = new();
    Vector3 _lastFollowPos = new();
    Camera cam;
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
        cam = GetComponent<Camera>();
    }
    void Start()
    {
        _lastFollowPos = follow.position;
        camZ = cam.transform.position.z;
        CameraLimiter = null;
    }
    void Update()
    {
        var _targetPos = follow.position + (follow.position - _lastFollowPos) * lookForward;

        if (Vector2.Distance(MinPoint, MaxPoint) > 0f && CameraLimiter != null)
        {
            _targetPos.x = Mathf.Clamp(_targetPos.x, MinPoint.x + Extents(cam).x, MaxPoint.x - Extents(cam).x);
            _targetPos.y = Mathf.Clamp(_targetPos.y, MinPoint.y + Extents(cam).y, MaxPoint.y - Extents(cam).y);
        }
        transform.position = Vector3.SmoothDamp(transform.position, _targetPos, ref _moveDamp, followSpeed);
        _lastFollowPos = follow.position;

        cam.transform.position = new(transform.position.x, transform.position.y, camZ);
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
