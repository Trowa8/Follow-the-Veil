using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Tooltip("The sprite to display in the inventory UI.")]
    public Sprite itemIcon;  // Assign a sprite in the Inspector (e.g., an icon for the item).

    [Tooltip("Optional: Name of the item.")]
    public string itemName = "Item";  // For future use, like tooltips.

    [Tooltip("The prefab to instantiate when dropping this item.")]
    public GameObject dropPrefab;  // Assign the item's prefab here (same as the one used to spawn it).

    // Optional: Add effects when picked up, e.g., sound or particle.
    public void OnPickup()
    {
        // Play a sound or effect here if desired.
        Destroy(gameObject);  // Remove the object from the scene after pickup.
    }
}