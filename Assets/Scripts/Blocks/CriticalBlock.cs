using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalBlock : MonoBehaviour
{
    [SerializeField] private float damage;
    private PlayerController _player;

    private void Awake()
    {
        var spr = GetComponent<SpriteRenderer>();
        if(spr != null) spr.enabled = false;
    }

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && _player.CurrentState != Enums.PlayerState.Respawn)
        {
            _player.playerHarmable.Harm(damage);
            _player.ReturnToSpawn();
        }
    }
}
