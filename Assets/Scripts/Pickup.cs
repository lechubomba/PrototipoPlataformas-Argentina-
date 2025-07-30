using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string itemID;
    public int amount = 1;
    public InventoryManager inventoryManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (inventoryManager.AddItem(itemID, amount))
                Destroy(gameObject);
        }
    }
}