using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance {get; private set;}
    public float PlayerMaxHealth { get; set; } = 10f;
    public float PlayerHealth { get; set; } = 10f;
    public float ShieldHealth { get; set; } = 10f;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else
        {
            Debug.LogError("There is more than one instance of PlayerStatsManager");
            Destroy(this);
        }
    }
}
