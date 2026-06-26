using UnityEngine;
using UnityEngine.InputSystem;

public class BlenderCameraController : MonoBehaviour
{
    [Header("Target & Distance")]
    public Transform target; 
    public float distance = 10f;
    public float minDistance = 2f;
    public float maxDistance = 30f;

    [Header("Speeds")]
    public float orbitSpeed = 0.2f;
    public float panSpeed = 0.05f;
    public float zoomSpeed = 1000f;

    private float currentX = 0f;
    private float currentY = 45f; 
    private Vector3 targetPosition;

    // A flag to remember if we are allowed to rotate the camera
    private bool canOrbitWithLeftClick = false;

    void Start()
    {
        if (target != null) targetPosition = target.position;
        else targetPosition = transform.position + transform.forward * distance;
    }

    void LateUpdate()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 1. SMART LEFT CLICK DETECTION
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the object we clicked has physics (like your Dice)
                if (hit.rigidbody != null)
                {
                    canOrbitWithLeftClick = false; // We clicked the dice! Don't move the camera.
                }
                else
                {
                    canOrbitWithLeftClick = true; // We clicked the floor. We can move the camera.
                }
            }
            else
            {
                canOrbitWithLeftClick = true; // We clicked the sky. We can move the camera.
            }
        }

        // Reset the flag when we let go of the mouse
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            canOrbitWithLeftClick = false;
        }


        // 2. PAN (Middle Mouse Button)
        if (Mouse.current.middleButton.isPressed)
        {
            Vector3 panTranslation = transform.right * (-mouseDelta.x * panSpeed) + transform.up * (-mouseDelta.y * panSpeed);
            targetPosition += panTranslation;
        }
        
        // 3. ORBIT (Left Click on background, OR Right Click anywhere)
        else if ((Mouse.current.leftButton.isPressed && canOrbitWithLeftClick) || Mouse.current.rightButton.isPressed)
        {
            currentX += mouseDelta.x * orbitSpeed;
            currentY -= mouseDelta.y * orbitSpeed;

            currentY = Mathf.Clamp(currentY, -85f, 85f); // Stop camera from flipping upside down
        }

        // 4. ZOOM (Scroll Wheel)
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            distance -= (scrollValue * zoomSpeed) * 0.001f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // 5. APPLY FINAL POSITION
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + targetPosition;

        transform.rotation = rotation;
        transform.position = position;
    }
}