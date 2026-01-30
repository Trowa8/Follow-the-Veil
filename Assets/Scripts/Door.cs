using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("The mask type required to open this door (e.g., 'Blue'). Checks if the selected item name contains this string.")]
    public string requiredMask = "Blue";
    
    [Tooltip("Distance the door moves when opening (in units).")]
    public float moveDistance = 5f;
    
    [Tooltip("Speed of movement (units per second).")]
    public float moveSpeed = 2f;
    
    [Tooltip("Direction of movement (default: down).")]
    public Vector3 moveDirection = Vector3.down;

    private Vector3 originalPosition;
    private bool isOpening = false;
    private float movedDistance = 0f;

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (isOpening)
        {
            float moveAmount = moveSpeed * Time.deltaTime;
            transform.Translate(moveDirection * moveAmount);
            movedDistance += moveAmount;

            if (movedDistance >= moveDistance)
            {
                isOpening = false;
                transform.position = originalPosition + moveDirection * moveDistance * 200f;
            }
        }
    }

    public void OpenDoor()
    {
        if (!isOpening)
        {
            isOpening = true;
            movedDistance = 0f;
        }
    }
}