using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el inventario de ítems recogidos, iconos y notifica a la UI cuando cambia.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Configuración Inventario")]
    [Tooltip("Máximo de ranuras en el inventario")]
    public int maxSlots = 6;

    [Header("Base de Datos de Ítems")]
    [Tooltip("Lista de mapeo de ID de ítem a Sprite para iconos generales")]
    public List<ItemData> itemsDatabase;

    /// <summary>
    /// Información de cada ítem: ID y Sprite.
    /// </summary>
    [System.Serializable]
    public struct ItemData
    {
        public string itemID;
        public Sprite icon;
    }

    private Dictionary<string, Sprite> iconLookup;

    [System.Serializable]
    public class Slot
    {
        [Tooltip("Identificador único del ítem (ej: \"Yerba\")")]
        public string itemID;
        [Tooltip("Cantidad apilada en este slot")]
        public int quantity;
    }

    [Tooltip("Lista de slots actuales en el inventario")]
    public List<Slot> slots = new List<Slot>();

    /// <summary>
    /// Evento que se dispara cuando el inventario cambia.
    /// </summary>
    public event System.Action onInventoryChanged;

    void Awake()
    {
        // Construir lookup de iconos
        iconLookup = new Dictionary<string, Sprite>();
        foreach (var data in itemsDatabase)
        {
            if (!iconLookup.ContainsKey(data.itemID))
                iconLookup.Add(data.itemID, data.icon);
        }
    }

    /// <summary>
    /// Agrega una cantidad de un ítem. Crea un nuevo slot si no existe.
    /// </summary>
    public bool AddItem(string id, int amount)
    {
        var existing = slots.Find(s => s.itemID == id);
        if (existing != null)
        {
            existing.quantity += amount;
            onInventoryChanged?.Invoke();
            return true;
        }
        if (slots.Count < maxSlots)
        {
            slots.Add(new Slot { itemID = id, quantity = amount });
            onInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Quita una cantidad de un ítem. Elimina el slot si la cantidad llega a cero.
    /// </summary>
    public bool RemoveItem(string id, int amount)
    {
        var existing = slots.Find(s => s.itemID == id);
        if (existing != null && existing.quantity >= amount)
        {
            existing.quantity -= amount;
            if (existing.quantity <= 0)
                slots.Remove(existing);
            onInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Comprueba si hay al menos cierta cantidad de un ítem.
    /// </summary>
    public bool HasItem(string id, int amount)
    {
        var existing = slots.Find(s => s.itemID == id);
        return existing != null && existing.quantity >= amount;
    }

    /// <summary>
    /// Devuelve la cantidad actual de un ítem en el inventario.
    /// </summary>
    public int GetQuantity(string id)
    {
        var slot = slots.Find(s => s.itemID == id);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// Devuelve una copia de la lista de slots.
    /// </summary>
    public List<Slot> GetSlots()
    {
        return new List<Slot>(slots);
    }

    /// <summary>
    /// Devuelve el sprite asociado a un ID de ítem.
    /// </summary>
    public Sprite GetIconForID(string id)
    {
        if (iconLookup != null && iconLookup.TryGetValue(id, out var spr))
            return spr;
        return null;
    }
}