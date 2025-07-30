using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la UI del inventario mostrando slots con icono y cantidad.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Referencia al gestor de inventario")]
    public InventoryManager inventoryManager;
    [Tooltip("Padre que contendrá los slots en el Canvas")]
    public Transform slotsParent;
    [Tooltip("Prefab de un slot con Image (Icon) y Text (Quantity)")]
    public GameObject slotPrefab;

    [Header("Base de Datos de Ítems")]
    [Tooltip("Lista de mapeo de ID de ítem a Sprite")]
    public List<ItemData> itemsDatabase;

    private Dictionary<string, Sprite> lookup;
    private List<GameObject> slotUIs = new List<GameObject>();

    [System.Serializable]
    public struct ItemData
    {
        public string itemID;
        public Sprite icon;
    }

    void Awake()
    {
        // Crear lookup para acceso rápido a sprites
        lookup = new Dictionary<string, Sprite>();
        foreach (var data in itemsDatabase)
        {
            if (!lookup.ContainsKey(data.itemID)) lookup.Add(data.itemID, data.icon);
        }
    }

    void Start()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no asignado en InventoryUI");
            return;
        }

        // Suscribirse al evento de cambio en inventario
        inventoryManager.onInventoryChanged += UpdateUI;

        // Instanciar slots según maxSlots
        for (int i = 0; i < inventoryManager.maxSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotsParent);
            slotUIs.Add(slotGO);
        }

        UpdateUI();
    }

    void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.onInventoryChanged -= UpdateUI;
    }

    /// <summary>
    /// Actualiza visualmente los slots con ítems del inventario.
    /// </summary>
    public void UpdateUI()
    {
        var slots = inventoryManager.GetSlots();
        for (int i = 0; i < slotUIs.Count; i++)
        {
            Image icon = slotUIs[i].transform.Find("Icon").GetComponent<Image>();
            Text qtyText = slotUIs[i].transform.Find("Quantity").GetComponent<Text>();

            if (i < slots.Count)
            {
                slotUIs[i].SetActive(true);
                string id = slots[i].itemID;
                if (lookup.TryGetValue(id, out Sprite spr))
                    icon.sprite = spr;
                else
                    icon.sprite = null; // o un sprite por defecto

                qtyText.text = slots[i].quantity.ToString();
            }
            else
            {
                slotUIs[i].SetActive(false);
            }
        }
    }
}
