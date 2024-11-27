using System.Collections.Generic;
using UnityEngine;
 
[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] ParallaxCamera parallaxCamera;
    private List<ParallaxLayer> parallaxLayers;
 
    void Start()
    {
        if (parallaxCamera == null)
            if (Camera.main != null)
                parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;
 
        SetLayers();
    }
 
    void SetLayers()
    {
        parallaxLayers = new();
 
        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();
 
            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }
 
    void Move(Vector2 delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}