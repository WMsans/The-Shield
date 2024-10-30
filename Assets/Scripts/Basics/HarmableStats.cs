using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Harmable Stats", menuName = "Custom Assets/Harmable Stats", order = 1)]
public class HarmableStats : ScriptableObject
{
    public List<string> harmableTags;
}
