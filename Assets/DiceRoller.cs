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
    public Camera mainCamera;
    public Camera povCamera;

    public event System.Action OnRollStart;
    public event System.Action<int> OnDiceLanded;

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
        mainCamera.enabled = true;
        povCamera.enabled = false;

    }

    public void RollDice()
    {
        if (isRolling) return;
        OnRollStart?.Invoke();
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
        if (resultText != null) resultText.text = "Rolling...";
        mainCamera.enabled = !mainCamera.enabled;
        povCamera.enabled = !povCamera.enabled;
        yield return new WaitForSeconds(0.2f);

        while (rb.linearVelocity.sqrMagnitude > 0.001f || rb.angularVelocity.sqrMagnitude > 0.001f)
        {
            yield return null;
        }

        int rolledNumber = GetTopFaceNumber();
        if (resultText != null)
            resultText.text = $"Rolled: {rolledNumber}";

        OnDiceLanded?.Invoke(rolledNumber);
        isRolling = false;
        mainCamera.enabled = !mainCamera.enabled;
        povCamera.enabled = !povCamera.enabled;
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