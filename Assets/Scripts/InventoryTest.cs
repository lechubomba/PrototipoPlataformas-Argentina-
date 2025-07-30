using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public InventoryManager inventoryManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            inventoryManager.AddItem("Yerba", 1);
        if (Input.GetKeyDown(KeyCode.U))
            inventoryManager.RemoveItem("Yerba", 1);
    }
}