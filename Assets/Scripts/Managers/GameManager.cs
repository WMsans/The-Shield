using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one GameManager in the scene!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(6, 7, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene( SceneManager.GetActiveScene().name );


        }
    }
}
