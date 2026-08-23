using System.Collections;
using UnityEngine;
using TMPro;

public class DiceRoller : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public TMP_Text resultText;

    // 1. ADD THIS: Reference to your player controller script
    public PlayerController playerScript;

    [Header("Speed Tweaks")]
    public float throwForce = 5f;
    public float maxTorque = 2500f;
    public float dropForce = 15f;
    public float maxSpinSpeed = 50f;

    private bool isRolling = false;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxSpinSpeed;
    }

    public void RollDice()
    {
        if (isRolling) return;

        transform.position = new Vector3(0, 3f, 0);
        transform.rotation = Random.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 force = new Vector3(
            Random.Range(-1f, 1f) * throwForce,
            -dropForce,
            Random.Range(-1f, 1f) * throwForce
        );
        rb.AddForce(force, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque)
        );
        rb.AddTorque(torque, ForceMode.Impulse);

        StartCoroutine(CheckResultWhenStopped());
    }

    private IEnumerator CheckResultWhenStopped()
    {
        isRolling = true;
        if (resultText != null) resultText.text = "Rolling...";

        yield return new WaitForSeconds(0.2f);

        while (rb.linearVelocity.sqrMagnitude > 0.001f || rb.angularVelocity.sqrMagnitude > 0.001f)
        {
            yield return null;
        }

        int rolledNumber = GetTopFaceNumber();
        if (resultText != null) resultText.text = "Rolled: " + rolledNumber;

        // 2. ADD THIS: Tell the player to move the calculated rolled number!
        if (playerScript != null)
        {
            playerScript.MoveSteps(rolledNumber);
        }

        isRolling = false;
    }

    private int GetTopFaceNumber()
    {
        (Vector3 direction, int value)[] faces = new (Vector3, int)[]
        {
            (transform.up, 2),        // Top face (+Y)
            (-transform.up, 5),       // Bottom face (-Y)
            (transform.right, 4),     // Right face (+X)
            (-transform.right, 3),    // Left face (-X)
            (transform.forward, 1),   // Front face (+Z)
            (-transform.forward, 6)
        };

        float highestDotProduct = -1f;
        int topValue = 1;

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