using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AstarPath))]
public class AstarManager : MonoBehaviour
{
    public static AstarManager Instance {get; private set;}
    private AstarPath _path;
    void Awake()
    {
        if(Instance == null) Instance = this;
        else
        {
            Debug.LogWarning("More than one instance of AstarManager in the scene");
            Destroy(gameObject);
            return;
        }
        _path = GetComponent<AstarPath>();
    }

    public void Rescan()
    {
        _path.Scan();
    }
}
