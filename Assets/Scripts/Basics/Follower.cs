using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed;
    private void LateUpdate()
    {
        if (!target) return;
        transform.position = Vector2.MoveTowards(transform.position, target.position, followSpeed * Time.deltaTime);
    }
}
