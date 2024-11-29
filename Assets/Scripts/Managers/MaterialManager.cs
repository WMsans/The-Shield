using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MaterialManager : MonoBehaviour
{
    public Material material;
    public abstract void UpdateMaterial(float start, float end, float duration, BetterLerp.LerpType type, bool invert = false);
}
