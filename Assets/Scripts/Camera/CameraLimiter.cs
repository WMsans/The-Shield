using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraLimiter : MonoBehaviour
{
    [Header("Camera Limit")]
    [SerializeField] Collider2D collisionBound;
    [SerializeField] Collider2D cameraBound;
    [Header("Scene Transition")]
    [SerializeField] List<SceneField> scenesToLoad;
    [SerializeField] List<Transform> respawnPoint;

    private CameraFollower _cameraFollower;
    private bool _enabled;
    private PlayerController _player;
    void Awake()
    {
        _enabled = false;

        var spr = GetComponent<SpriteRenderer>();
        if(spr) spr.enabled = false;
    }
    private void Start()
    {
        _cameraFollower = CameraFollower.Instance;
        _player = PlayerController.Instance;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collisionBound.bounds.Contains(collision.gameObject.transform.position))
            {
                EnableLimit();
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

    void EnableLimit()
    {
        if (_enabled) return;
        _enabled = true;
        _cameraFollower.CameraLimiter = this;
        //Make this the limiter
        _cameraFollower.Limitin(cameraBound.bounds.min, cameraBound.bounds.max);
        // Set respawn point for player
        _player.RespawnPoint = respawnPoint.OrderBy(t=>Vector2.Distance(t.position, _player.transform.position)).FirstOrDefault()!.position;
        Debug.Log(_player.RespawnPoint);
        // Load scenes
        UpdateScenes();
    }
    void UnEnableLimit()
    {
        if(!_enabled) return;
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
            if (loadedScene.name == "PersistantScene" || loadedScene.name == "DontDestroyOnLoad" || loadedScene == gameObject.scene) continue;
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
    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(collisionBound.bounds.center, collisionBound.bounds.size);  
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(cameraBound.bounds.center, cameraBound.bounds.size);
        Gizmos.color = new Color(255, 165, 0);
        foreach(var i in respawnPoint)
            Gizmos.DrawWireSphere(i.position, 1f);
    }
    #endif
}
