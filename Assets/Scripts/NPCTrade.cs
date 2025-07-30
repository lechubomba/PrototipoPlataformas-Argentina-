using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Controla un NPC est�tico que ofrece un trueque de �tems.
/// </summary>
public class NPCTrade : MonoBehaviour
{
    [Header("Configuraci�n Trueque")]
    [Tooltip("ID de los �tems que el NPC pide")]
    public List<string> requiredItemIDs;

    [Tooltip("Cantidad de cada �tem requerido (mismo orden que requiredItemIDs)")]
    public List<int> requiredAmounts;

    [Header("Recompensa")]
    [Tooltip("ID de la habilidad a desbloquear tras trueque")]
    public string rewardAbilityID;
    [Tooltip("Nombre legible de la habilidad para mostrar en UI")]
    public string rewardAbilityName;

    [Header("Referencias a Managers")]
    [Tooltip("Referencia al InventoryManager en escena")]
    public InventoryManager inventoryManager;

    [Tooltip("Panel de UI que se muestra al interactuar")]
    public GameObject tradePanel;

    [Tooltip("Prefab de l�nea de requerimiento: icono, TxtHave, TxtNeed")]
    public GameObject lineItemPrefab;

    [Tooltip("Padre de las l�neas dentro del panel")]
    public Transform linesParent;

    [Header("UI adicional")]
    [Tooltip("Texto donde mostrar el nombre de la habilidad")]
    public Text txtAbilityName;

    [Header("Tecla de interacci�n")]
    [Tooltip("Tecla para abrir/cerrar el panel de trueque")]
    public KeyCode interactKey = KeyCode.E;

    private bool playerNearby;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleTradePanel(true);
            PopulateTradeUI();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            ToggleTradePanel(false);
        }
    }

    /// <summary>
    /// Muestra u oculta el panel de trueque.
    /// </summary>
    void ToggleTradePanel(bool visible)
    {
        if (txtAbilityName != null)
        {
            txtAbilityName.text = 
                (string.IsNullOrEmpty(rewardAbilityName) ? rewardAbilityID : rewardAbilityName);
        }
        if (tradePanel != null)
            tradePanel.SetActive(visible);
    }

    /// <summary>
    /// Llena la UI con los �tems requeridos y configura botones.
    /// </summary>
    void PopulateTradeUI()
    {
        if (tradePanel == null || lineItemPrefab == null || linesParent == null)
            return; // Referencias faltantes

        // Limpia l�neas previas
        foreach (Transform child in linesParent)
            Destroy(child.gameObject);

        // Crea una l�nea por cada �tem requerido
        for (int i = 0; i < requiredItemIDs.Count; i++)
        {
            GameObject line = Instantiate(lineItemPrefab, linesParent);
            Image icon = line.transform.Find("Icon").GetComponent<Image>();
            Text txtHave = line.transform.Find("TxtHave").GetComponent<Text>();
            Text txtNeed = line.transform.Find("TxtNeed").GetComponent<Text>();

            string id = requiredItemIDs[i];
            int need = requiredAmounts[i];
            int have = inventoryManager.GetQuantity(id);

            // Asigna sprite y corrige el tinte
            icon.sprite = inventoryManager.GetIconForID(id);
            icon.color = Color.white;

            // Muestra valores reales
            txtHave.text = have.ToString();
            txtNeed.text = need.ToString();
        }

        // Configura botones Confirmar y Cancelar
        Button btnConfirm = tradePanel.transform.Find("BtnConfirm").GetComponent<Button>();
        Button btnCancel = tradePanel.transform.Find("BtnCancel").GetComponent<Button>();
        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.AddListener(OnConfirmTrade);
        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(() => ToggleTradePanel(false));
    }

    /// <summary>
    /// Confirma el trueque: comprueba y consume �tems, otorga recompensa.
    /// </summary>
    void OnConfirmTrade()
    {
        // Verificar requisitos
        for (int i = 0; i < requiredItemIDs.Count; i++)
        {
            if (!inventoryManager.HasItem(requiredItemIDs[i], requiredAmounts[i]))
            {
                Debug.Log("No tienes suficientes " + requiredItemIDs[i]);
                return;
            }
        }

        // Consumir �tems
        for (int i = 0; i < requiredItemIDs.Count; i++)
            inventoryManager.RemoveItem(requiredItemIDs[i], requiredAmounts[i]);

        // Otorgar habilidad
        GameManager.Instance.GrantAbility(rewardAbilityID);

        // Cerrar panel
        ToggleTradePanel(false);
        Debug.Log("Trueque completado: " + rewardAbilityID + " desbloqueada");
    }
}
