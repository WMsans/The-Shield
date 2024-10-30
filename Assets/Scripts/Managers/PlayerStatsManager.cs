using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance {get; private set;}
    [SerializeField] private TMP_Text playerHPtext;
    [SerializeField] private TMP_Text shieldHPtext;
    public float PlayerMaxHealth { get; set; } = 10f;
    public float PlayerHealth { get; set; } = 10f;
    public float ShieldHealth { get; set; } = 10f;
    public float PlayerAttack { get; set; } = 4f;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else
        {
            Debug.LogError("There is more than one instance of PlayerStatsManager");
            Destroy(this);
        }
    }

    void Update()
    {
        playerHPtext.text = "HP: " + PlayerHealth;
        shieldHPtext.text = "Shield HP: " + ShieldHealth;
    }
}
