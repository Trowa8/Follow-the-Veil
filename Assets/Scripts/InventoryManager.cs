using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct InventoryItem
{
    public Sprite sprite;
    public GameObject dropPrefab;
    public string itemName;

    public InventoryItem(Sprite sprite, GameObject dropPrefab, string itemName)
    {
        this.sprite = sprite;
        this.dropPrefab = dropPrefab;
        this.itemName = itemName;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Tooltip("List of UI slots (Images) for the inventory.")]
    public List<Image> inventorySlots = new List<Image>();

    [Tooltip("TMP_Text label to display the name of the selected item.")]
    public TMP_Text selectedItemNameLabel;

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    private int selectedSlot = 0;

    public int SelectedSlot => selectedSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(InteractableItem item)
    {
        if (inventoryItems.Count < inventorySlots.Count)
        {
            InventoryItem newItem = new InventoryItem(item.itemIcon, item.dropPrefab, item.itemName);
            inventoryItems.Add(newItem);
            UpdateUI();
            return true;
        }
        return false;
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < inventoryItems.Count)
        {
            inventoryItems.RemoveAt(index);
            UpdateUI();
        }
    }

    public InventoryItem DropItem()
    {
        if (selectedSlot >= 0 && selectedSlot < inventoryItems.Count && inventoryItems[selectedSlot].dropPrefab != null)
        {
            InventoryItem droppedItem = inventoryItems[selectedSlot];
            inventoryItems.RemoveAt(selectedSlot);
            UpdateUI();
            return droppedItem;
        }
        return default;
    }

    public void SelectSlot(int slot)
    {
        if (slot >= 0 && slot < inventorySlots.Count)
        {
            selectedSlot = slot;
            UpdateUI();
        }
    }

    public void SelectNextSlot()
    {
        selectedSlot = (selectedSlot + 1) % inventorySlots.Count;
        UpdateUI();
    }

    public void SelectPreviousSlot()
    {
        selectedSlot = (selectedSlot - 1 + inventorySlots.Count) % inventorySlots.Count;
        UpdateUI();
    }
    public string GetSelectedItemName()
    {
        if (selectedSlot >= 0 && selectedSlot < inventoryItems.Count)
        {
            return inventoryItems[selectedSlot].itemName;
        }
        return "";
    }

    public InventoryItem GetSelectedItem()
    {
       if (selectedSlot >= 0 && selectedSlot < inventoryItems.Count)
        {
           return inventoryItems[selectedSlot];
        }
        return default;
    }
    private void UpdateUI()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < inventoryItems.Count)
            {
                inventorySlots[i].sprite = inventoryItems[i].sprite;
                inventorySlots[i].enabled = true;
            }
            else
            {
                inventorySlots[i].sprite = null;
                inventorySlots[i].enabled = false;
            }

            Image parentImage = inventorySlots[i].transform.parent?.GetComponent<Image>();
            if (parentImage != null)
            {
                if (i == selectedSlot)
                {
                    parentImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    parentImage.color = Color.white;
                }
            }
            else
            {
                Debug.LogWarning($"Slot {i} has no parent Image component");
            }
        }

        if (selectedItemNameLabel != null)
        {
            if (selectedSlot >= 0 && selectedSlot < inventoryItems.Count && !string.IsNullOrEmpty(inventoryItems[selectedSlot].itemName))
            {
                selectedItemNameLabel.text = inventoryItems[selectedSlot].itemName;
            }
            else
            {
                selectedItemNameLabel.text = "";
            }
        }
        else
        {
            Debug.LogWarning("Selected Item Name Label is not assigned in InventoryManager.");
        }
    }
}