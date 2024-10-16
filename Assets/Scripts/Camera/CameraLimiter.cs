using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraLimiter : MonoBehaviour
{
    [SerializeField] Collider2D collisionBound;
    [SerializeField] Collider2D cameraBound;
    [SerializeField] SceneField[] scenesToLoad;

    private CameraFollower _cameraFollower;
    private bool _enabled;
    void Awake()
    {
        _enabled = false;

        var spr = GetComponent<SpriteRenderer>();
        if(spr) spr.enabled = false;
    }
    private void Start()
    {
        _cameraFollower = CameraFollower.Instance;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _enabled = true;
            if (collisionBound.bounds.Contains(collision.gameObject.transform.position)){
                _cameraFollower.CameraLimiter = this;
                //Make this the limiter
                _cameraFollower.Limitin(cameraBound.bounds.min, cameraBound.bounds.max);
                // Load scenes
                UpdateScenes();
            }
            else
            {
                UnEnableLimit();
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && _enabled)
        {
            UnEnableLimit();
        }
    }

    void UnEnableLimit()
    {
        _enabled = false;
        if (_cameraFollower.CameraLimiter == this)
        {
            _cameraFollower.CameraLimiter = null;
            _cameraFollower.Limitout();
        }
    }
    void UpdateScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == "PersistentGameplay" || loadedScene.name == "DontDestroyOnLoad") continue;
            var unloading = true;
            foreach (var scene in scenesToLoad)
            {
                if(scene.SceneName == loadedScene.name)
                {
                    unloading = false;
                    break;
                }
            }
            if(unloading) SceneManager.UnloadSceneAsync(loadedScene);
        }
        foreach (var scene in scenesToLoad)
        {
            bool isSceneLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name == scene.SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }
            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            }
        }
    }
}
