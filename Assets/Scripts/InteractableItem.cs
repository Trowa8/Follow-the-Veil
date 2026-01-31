using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Tooltip("The sprite to display in the inventory UI.")]
    public Sprite itemIcon;

    [Tooltip("Optional: Name of the item.")]
    public string itemName = "Item";

    [Tooltip("The prefab to instantiate when dropping this item.")]
    public GameObject dropPrefab; 

    [Tooltip("Whether this item can be picked up.")]
    public bool canPickup = true;

    public void OnPickup()
    {
        Destroy(gameObject); 
    }

    public void DisablePickup()
    {
        canPickup = false;
    }
}