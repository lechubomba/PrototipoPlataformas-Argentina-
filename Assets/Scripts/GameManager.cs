using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor global de juego: controla habilidades desbloqueadas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Lista de habilidades desbloqueadas
    private HashSet<string> abilities = new HashSet<string>();

    void Awake()
    {
        // Implementación de singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Devuelve una copia de las habilidades desbloqueadas.
    /// </summary>
    public List<string> GetAbilities()
    {
        return new List<string>(abilities);
    }

    /// <summary>
    /// Desbloquea una habilidad específica.
    /// </summary>
    public void GrantAbility(string abilityID)
    {
        if (!abilities.Contains(abilityID))
        {
            abilities.Add(abilityID);
            Debug.Log("Habilidad desbloqueada: " + abilityID);
        }
    }

    /// <summary>
    /// Consulta si una habilidad ya fue desbloqueada.
    /// </summary>
    public bool HasAbility(string abilityID)
    {
        return abilities.Contains(abilityID);
    }
}
