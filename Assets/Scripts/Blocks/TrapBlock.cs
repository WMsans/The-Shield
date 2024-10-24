using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TrapBlock : MonoBehaviour
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
        if (other.CompareTag("Player"))
        {
            _player.Harm(damage);
        }
    }
}
