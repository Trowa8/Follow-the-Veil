using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private float minLookAngle = -90f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private TMP_Text interactText;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction slot1Action;
    private InputAction slot2Action;
    private InputAction slot3Action;
    private InputAction scrollWheelAction;
    private InputAction dropAction;
    private InputAction useItemAction; // New: For LMB (UseItem action)

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private Vector3 horizontalVelocity;
    private float xRotation = 0f;

    // New: For mask preview
    private GameObject currentMaskClone = null;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        var playerActionMap = inputActions.FindActionMap("Player");
        moveAction = playerActionMap.FindAction("Move");
        lookAction = playerActionMap.FindAction("Look");
        jumpAction = playerActionMap.FindAction("Jump");
        runAction = playerActionMap.FindAction("Sprint");
        interactAction = playerActionMap.FindAction("Interact");
        slot1Action = playerActionMap.FindAction("Slot1");
        slot2Action = playerActionMap.FindAction("Slot2");
        slot3Action = playerActionMap.FindAction("Slot3");
        dropAction = playerActionMap.FindAction("Drop");
        useItemAction = playerActionMap.FindAction("Attack"); // New: Add this action in your InputActionAsset

        // New: Find the UI action map and its ScrollWheel action.
        var uiActionMap = inputActions.FindActionMap("UI");
        scrollWheelAction = uiActionMap.FindAction("ScrollWheel");

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        runAction.Enable();
        interactAction.Enable();
        slot1Action.Enable();
        slot2Action.Enable();
        slot3Action.Enable();
        scrollWheelAction.Enable();
        dropAction.Enable();
        useItemAction.Enable(); // New: Enable the UseItem action

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        runAction.Disable();
        interactAction.Disable();
        slot1Action.Disable();
        slot2Action.Disable();
        slot3Action.Disable();
        scrollWheelAction.Disable();
        dropAction.Disable();
        useItemAction.Disable(); // New: Disable the UseItem action
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleInteraction();
        HandleSlotSelection();
        HandleMaskPreview(); // New: Handle mask preview logic
        UpdateInteractText();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        float speed = runAction.IsPressed() ? runSpeed : walkSpeed;

        Vector3 targetHorizontalVelocity = (transform.right * moveX + transform.forward * moveZ) * speed;

        if (IsGrounded())
        {
            float accelRate = targetHorizontalVelocity.magnitude > 0.1f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontalVelocity, accelRate * Time.deltaTime);
        }

        controller.Move(horizontalVelocity * Time.deltaTime);

        if (IsGrounded() && jumpAction.WasPressedThisFrame())
        {
            velocity.y = jumpForce;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleInteraction()
    {
        if (interactAction.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Change: Use ~0 to check all layers, not just interactableLayer
        if (Physics.Raycast(ray, out hit, interactionDistance, ~0))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null && item.canPickup)  // Modified: Only pick up if canPickup is true
            {
                if (InventoryManager.Instance.AddItem(item))
                {
                    item.OnPickup();
                    Debug.Log("Picked up: " + item.itemName);
                }
            }
            // If no InteractableItem component or canPickup is false, it hit something else (like a wall), so do nothing
        }
    }

    private void HandleSlotSelection()
    {
        Vector2 scrollInput = scrollWheelAction.ReadValue<Vector2>();
        float scrollY = scrollInput.y;
        if (scrollY > 0)
        {
            InventoryManager.Instance.SelectNextSlot();
        }
        else if (scrollY < 0)
        {
            InventoryManager.Instance.SelectPreviousSlot();
        }

        if (slot1Action.WasPressedThisFrame())
        {
            InventoryManager.Instance.SelectSlot(0);
        }
        if (slot2Action.WasPressedThisFrame())
        {
            InventoryManager.Instance.SelectSlot(1);
        }
        if (slot3Action.WasPressedThisFrame())
        {
            InventoryManager.Instance.SelectSlot(2);
        }

        if (dropAction.WasPressedThisFrame())
        {
            InventoryItem itemToDrop = InventoryManager.Instance.DropItem();
            if (itemToDrop.dropPrefab != null)
            {
                DropItemInWorld(itemToDrop);
            }
            else
            {
                Debug.Log("No item to drop.");
            }
        }
    }

    // New: Method to handle mask preview
    private void HandleMaskPreview()
    {
        string selectedItemName = InventoryManager.Instance.GetSelectedItemName();
        bool isMask = !string.IsNullOrEmpty(selectedItemName) && selectedItemName.Contains("Mask") || selectedItemName.Contains("Key");

        if (isMask && useItemAction.IsPressed())
        {
            // Existing: Create/update clone
            if (currentMaskClone == null)
            {
                InventoryItem selectedItem = InventoryManager.Instance.GetSelectedItem();
                if (selectedItem.dropPrefab != null)
                {
                    currentMaskClone = Instantiate(selectedItem.dropPrefab);
                    Collider[] colliders = currentMaskClone.GetComponentsInChildren<Collider>();
                    foreach (Collider col in colliders)
                    {
                        col.enabled = false;
                    }
                }
            }

            if (currentMaskClone != null)
            {
                Vector3 offsetPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.15f + Vector3.up * -0.35f;
                currentMaskClone.transform.position = offsetPosition;
                currentMaskClone.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward * 0.1f);
            }

            // New: Check for doors to open
            Door[] doors = FindObjectsOfType<Door>();
            foreach (Door door in doors)
            {
                float distance = Vector3.Distance(transform.position, door.transform.position);
                if (distance <= 2f && selectedItemName.Contains(door.requiredMask))
                {
                    door.OpenDoor();
                }
            }
        }
        else
        {
            if (currentMaskClone != null)
            {
                Destroy(currentMaskClone);
                currentMaskClone = null;
            }
        }
    }
    private void DropItemInWorld(InventoryItem itemData)
    {
        if (itemData.dropPrefab != null)
        {
            Vector3 intendedDropPosition = transform.position + transform.forward * 2f;
            Vector3 actualDropPosition = intendedDropPosition;

            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, 2f, ~0))
            {
                actualDropPosition = transform.position + Vector3.up * 2f;
            }

            GameObject droppedItem = Instantiate(itemData.dropPrefab, actualDropPosition, Quaternion.identity);
            InteractableItem itemScript = droppedItem.GetComponent<InteractableItem>();
            if (itemScript != null)
            {
                itemScript.itemIcon = itemData.sprite;
                itemScript.itemName = itemData.itemName;
                itemScript.dropPrefab = itemData.dropPrefab;
            }
        }
    }
    private void UpdateInteractText()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null)
            {
                interactText.text = item.itemName;
                return;
            }
        }

        interactText.text = "";
    }

    private bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + controller.center;
        return Physics.Raycast(rayOrigin, Vector3.down, controller.height / 2 + groundCheckDistance, groundLayer);
    }
}