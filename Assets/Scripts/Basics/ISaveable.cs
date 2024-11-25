using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    public void SaveData();
    public void LoadData();
    [ContextMenu("Generate Guid")]
    public void GenerateGuid();
    public static void GenerateGuids(ref string guids) => guids = System.Guid.NewGuid().ToString();
}
