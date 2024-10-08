using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [Header("Movements")]
    [SerializeField] Transform target;
    [SerializeField] Transform start;
    [SerializeField] float moveTime;
    [SerializeField] float returnTime;
    [Header("Movement Curve")]
    [SerializeField] BetterLerp.LerpType movementType;
    [SerializeField] bool inversed;
    private float _time;
    private Enums.MovingBlockState _state;
    
}
