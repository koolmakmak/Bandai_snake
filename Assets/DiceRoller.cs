using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceRoller : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public TMP_Text resultText;
    public GameObject drop_point;
    public GameObject cameraOne;
    public GameObject cameraTwo;

    [Header("Turn Management")]
    // List of all players participating in the game
    public List<PlayerController> players = new List<PlayerController>();
    private int currentPlayerIndex = 0;

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
        cameraOne.SetActive(true);
        cameraTwo.SetActive(false);
    }

    public void RollDice()
    {
        if (isRolling || players.Count == 0) return;
        Vector3 worldPosition = drop_point.transform.position;
        transform.position = worldPosition;
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
        cameraOne.SetActive(!cameraOne.activeSelf);
        cameraTwo.SetActive(!cameraTwo.activeSelf);
        if (resultText != null) resultText.text = "Rolling...";

        yield return new WaitForSeconds(0.2f);

        while (rb.linearVelocity.sqrMagnitude > 0.001f || rb.angularVelocity.sqrMagnitude > 0.001f)
        {
            yield return null;
        }

        int rolledNumber = GetTopFaceNumber();
        if (resultText != null)
            resultText.text = $"P{currentPlayerIndex + 1} Rolled: {rolledNumber}";

        // Move the active player
        if (players[currentPlayerIndex] != null)
        {
            players[currentPlayerIndex].MoveSteps(rolledNumber);
        }

        // Pass turn to the next player
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        yield return new WaitForSeconds(1f);
        cameraOne.SetActive(!cameraOne.activeSelf);
        cameraTwo.SetActive(!cameraTwo.activeSelf);
        isRolling = false;
    }

    private int GetTopFaceNumber()
    {
        (Vector3 direction, int value)[] faces = new (Vector3, int)[]
        {
            (transform.up, 2),
            (-transform.up, 5),
            (transform.right, 4),
            (-transform.right, 3),
            (transform.forward, 1),
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