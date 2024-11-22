using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BetterLerp
{
    public enum LerpType
    {
        Linear,
        Sin, 
        Square, 
        Cube, 
        Quart,
        Smoothstep,
        Smootherstep
    }

    public static float Lerp(float start, float end, float t, LerpType type, bool invert = false)
    {
        return Mathf.Lerp(start, end, invert ? InterpolateInvert(t, type) : Interpolate(t, type));
    }
    public static Vector2 Lerp(Vector2 start, Vector2 end, float t, LerpType type, bool invert = false)
    {
        return Vector2.Lerp(start, end, invert ? InterpolateInvert(t, type) : Interpolate(t, type));
    }
    public static Vector3 Lerp(Vector3 start, Vector3 end, float t, LerpType type, bool invert = false)
    {
        return Vector3.Lerp(start, end, invert ? InterpolateInvert(t, type) : Interpolate(t, type));
    }
    private static float Interpolate(float t, LerpType type)
    {
        switch (type)
        {
            case LerpType.Linear:
                return t;
            case LerpType.Sin:
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case LerpType.Square:
                return t * t;
            case LerpType.Cube:
                return t * t * t;
            case LerpType.Quart:
                return t * t * t * t;
            case LerpType.Smoothstep:
                return t * t * (3f - 2f * t);
            case LerpType.Smootherstep:
                return t * t * t * (t * (6f * t - 15f) + 10f);
            default:
                return t; // Default to linear interpolation
        }
    }

    private static float InterpolateInvert(float t, LerpType type)
    {
        switch (type)
        {
            case LerpType.Linear:
                return t;
            case LerpType.Sin:
                return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            case LerpType.Square:
                return Mathf.Sqrt(t);
            case LerpType.Cube:
                return Mathf.Pow(t, 1f / 3f);
            case LerpType.Quart:
                return Mathf.Pow(t, 0.25f);
            case LerpType.Smoothstep:
                return 0.5f - Mathf.Sin(Mathf.Asin(1f - 2f * t) / 3f);
            case LerpType.Smootherstep:
                return 0.5f - Mathf.Sin(Mathf.Asin(1f - 2f * t) / 3f); // Fallback to Smoothstep interpolation
            default:
                return t;
        }
    }
}
