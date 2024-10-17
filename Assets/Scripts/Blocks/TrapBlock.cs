using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TrapBlock : MonoBehaviour
{
    [SerializeField] private float damage;
    private IHarmable _player;

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _player.Harm(damage);
        }
    }
}
