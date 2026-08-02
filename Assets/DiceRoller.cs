using System.Collections;
using UnityEngine;
using TMPro; // Use UnityEngine.UI if using standard Text

public class DiceRoller : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public TMP_Text resultText; // UI Text element to show result

    [Header("Roll Parameters")]
    public float throwForce = 8f;
    public float maxTorque = 500f;

    private bool isRolling = false;

    // Call this method from your UI Button OnClick() event
    public void RollDice()
    {
        if (isRolling) return;

        // Reset position slightly above the floor and randomize starting rotation
        transform.position = new Vector3(0, 2f, 0);
        transform.rotation = Random.rotation;

        // Clear existing velocity
        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        // Apply a random upward/outward force
        Vector3 randomForce = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized * throwForce;
        rb.AddForce(randomForce, ForceMode.Impulse);

        // Apply random spin torque
        Vector3 randomTorque = new Vector3(
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        StartCoroutine(CheckResultWhenStopped());
    }

    private IEnumerator CheckResultWhenStopped()
    {
        isRolling = true;
        if (resultText != null) resultText.text = "Rolling...";

        // Wait a brief moment for physics forces to take effect
        yield return new WaitForSeconds(0.5f);

        // Wait until the die stops moving completely
        while (rb.linearVelocity.sqrMagnitude > 0.001f || rb.angularVelocity.sqrMagnitude > 0.001f)
        {
            yield return null;
        }

        // Calculate and display landed side
        int rolledNumber = GetTopFaceNumber();
        if (resultText != null) resultText.text = "Rolled: " + rolledNumber;

        isRolling = false;
    }

    private int GetTopFaceNumber()
    {
        // Define local vectors for each face of a standard Unity Cube
        // ADJUST THE NUMBERS (1-6) TO MATCH YOUR CUBE'S TEXTURE / DOT LAYOUT
        (Vector3 direction, int value)[] faces = new (Vector3, int)[]
        {
            (transform.up, 2),        // Top face (+Y)
            (-transform.up, 5),       // Bottom face (-Y)
            (transform.right, 4),     // Right face (+X)
            (-transform.right, 3),    // Left face (-X)
            (transform.forward, 1),   // Front face (+Z)
            (-transform.forward, 6)   // Back face (-Z)
        };

        float highestDotProduct = -1f;
        int topValue = 1;

        // Compare each face vector against World Up (Vector3.up)
        foreach (var face in faces)
        {
            float dotProduct = Vector3.Dot(face.direction, Vector3.up);
            if (dotProduct > highestDotProduct)
            {
                highestDotProduct = dotProduct;
                topValue = face.value;
            }
        }

        return topValue;
    }
}