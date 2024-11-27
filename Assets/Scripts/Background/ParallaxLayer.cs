using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public Vector2 parallaxFactor = Vector2.one;
    [SerializeField] GameObject cam;
    [SerializeField] bool infiniteParallax;
    private Vector2 _length;

    void Awake()
    {
        if(infiniteParallax)
            _length = GetComponent<SpriteRenderer>().bounds.size;
    }

    void Start()
    {
        if (cam == null)
            if(Camera.main != null)
                cam = Camera.main.gameObject;
    }
    public void Move(Vector2 delta)
    {
        Vector2 newPos = transform.localPosition;
        newPos -= delta * parallaxFactor;
        
        var movement = (Vector2)cam.transform.position - newPos;
        if (movement.x >= _length.x)
        {
            newPos.x += _length.x;
        }
        else if (movement.x <= -_length.x)
        {
            newPos.x -= _length.x;
        }

        if (movement.y >= _length.y)
        {
            newPos.y += _length.y;
        }
        else if (movement.y <= -_length.y)
        {
            newPos.y -= _length.y;
        }
        transform.localPosition = newPos;
    }
}