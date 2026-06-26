using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class dice_physic : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForce = 1f;
    public float spinForce = 10f;
    
    private Rigidbody rb;
    private bool isDragging = false;
    private bool hasRolled = false;

    // Variables for calculating mouse velocity
    private Vector3 mouseOffset;
    private float zCoord;
    private Vector3 lastMousePosition;
    private Vector3 throwVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDragging)
        {
            // Calculate how fast the mouse is moving to determine throw strength
            Vector3 currentMousePosition = GetMouseWorldPos();
            throwVelocity = (currentMousePosition - lastMousePosition) / Time.deltaTime;
            lastMousePosition = currentMousePosition;
        }
        else if (hasRolled)
        {
            // Check if the dice has almost completely stopped moving and spinning
            if (rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
            {
                DetermineDiceFace();
                hasRolled = false; // Reset so we only print the result once per throw
            }
        }
    }

    // 1. PICK UP THE DICE
    void OnMouseDown()
    {
        isDragging = true;
        hasRolled = false;
        
        rb.useGravity = false;
        rb.isKinematic = true; // Stop physics from interfering while holding it
        
        // Calculate depth and offset so the dice follows the mouse perfectly
        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mouseOffset = gameObject.transform.position - GetMouseWorldPos();
        lastMousePosition = GetMouseWorldPos();
    }

    // 2. MOVE THE DICE
    void OnMouseDrag()
    {
        // Update the dice position to follow the mouse
        transform.position = GetMouseWorldPos() + mouseOffset;
    }

    // 3. THROW THE DICE
    void OnMouseUp()
    {
        isDragging = false;
        rb.isKinematic = false; // Turn physics back on
        rb.useGravity = true;
        
        // Apply the calculated mouse velocity as a physical force
        rb.AddForce(throwVelocity * throwForce, ForceMode.Impulse);
        
        // Add random spin to make the roll look natural
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rb.AddTorque(randomTorque * spinForce, ForceMode.Impulse);
        
        hasRolled = true;
    }

    // Helper to translate 2D screen mouse position into 3D world space
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // 4. READ THE RESULT
    private void DetermineDiceFace()
    {
        float maxDot = -Mathf.Infinity;
        int winningFace = 0;

        // Map local directions to dice numbers.
        // NOTE: You may need to change these numbers based on how your 3D model's texture is mapped!
        (Vector3 direction, int number)[] faces = new (Vector3, int)[]
        {
            (transform.up, 1),           // Top
            (-transform.up, 6),          // Bottom
            (transform.right, 2),        // Right
            (-transform.right, 5),       // Left
            (transform.forward, 3),      // Front
            (-transform.forward, 4)      // Back
        };

        // Loop through all 6 directions to see which one points closest to World Up
        foreach (var face in faces)
        {
            float dotProduct = Vector3.Dot(face.direction, Vector3.up);
            if (dotProduct > maxDot)
            {
                maxDot = dotProduct;
                winningFace = face.number;
            }
        }

        Debug.Log($"The dice rolled a: {winningFace}");
    }
}