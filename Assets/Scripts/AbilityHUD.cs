using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra en pantalla, en la esquina inferior derecha, un listado de habilidades desbloqueadas.
/// </summary>
public class AbilityHUD : MonoBehaviour
{
    [System.Serializable]
    public struct AbilityData
    {
        [Tooltip("Identificador de la habilidad (ID)")]
        public string abilityID;
        [Tooltip("Sprite asociado a esta habilidad")]
        public Sprite icon;
    }

    [Header("Configuración HUD")]
    [Tooltip("Prefab de icono de habilidad: debe llevar un componente Image sin sprite asignado.")]
    public GameObject abilityIconPrefab;
    [Tooltip("Parent UI (RectTransform) en la esquina inferior derecha donde instanciar los iconos.")]
    public RectTransform iconsParent;

    [Header("Base de datos de habilidades")]
    [Tooltip("Lista de mapeo de abilityID a Sprite para los iconos")]
    public List<AbilityData> abilityDatabase;

    private Dictionary<string, Sprite> iconLookup;
    private HashSet<string> displayed = new HashSet<string>();

    void Awake()
    {
        // Construir lookup de iconos
        iconLookup = new Dictionary<string, Sprite>();
        foreach (var data in abilityDatabase)
        {
            if (!iconLookup.ContainsKey(data.abilityID))
                iconLookup.Add(data.abilityID, data.icon);
        }
    }

    void Update()
    {
        // Obtener lista actual de habilidades desbloqueadas
        var abilities = GameManager.Instance.GetAbilities();
        foreach (var abilityID in abilities)
        {
            if (displayed.Contains(abilityID))
                continue;

            displayed.Add(abilityID);
            Sprite sprite;
            if (!iconLookup.TryGetValue(abilityID, out sprite))
            {
                Debug.LogWarning($"[AbilityHUD] No se encontró sprite para abilityID '{abilityID}'");
                continue;
            }

            // Instanciar icono
            var go = Instantiate(abilityIconPrefab, iconsParent);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
        }
    }
}
