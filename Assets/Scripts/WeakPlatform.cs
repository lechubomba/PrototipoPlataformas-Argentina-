using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente para plataformas fr�giles que se rompen al Ground Pound.
/// </summary>
public class WeakPlatform : MonoBehaviour
{
    [Tooltip("Cantidad de impactos necesarios para romper la plataforma")] public int durability = 1;
    [Tooltip("Part�culas al romperse")] public ParticleSystem breakEffect;

    /// <summary>
    /// Reduce durabilidad y destruye cuando llega a cero.
    /// </summary>
    public void Break()
    {
        durability--;
        if (breakEffect)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }
        if (durability <= 0)
        {
            Destroy(gameObject);
        }
    }
}
