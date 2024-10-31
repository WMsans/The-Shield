using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistant : IResetable
{
    public string Id { get; protected set; }
    public void SaveData();
    public void LoadData();
    [ContextMenu("Generate Guid")]
    public void GenerateGuid();
    public static void GenerateGuids(ref string guids) => guids = System.Guid.NewGuid().ToString();
}
