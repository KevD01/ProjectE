using System;
using UnityEngine;

public static class NoiseSystem
{
    public static event Action<Vector3, float> OnNoiseEmitted;

    public static void EmitNoise(Vector3 position, float range)
    {
        OnNoiseEmitted?.Invoke(position, range);

        Debug.Log("Ruido emitido en " + position + " con rango " + range);
    }
}