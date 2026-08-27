using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BoardGameTurnManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera defaultCamera;
    [SerializeField] private CinemachineCamera zoomCamera;

    [Header("Players & UI")]
    [SerializeField] private List<Transform> players; // 4 Player Transforms
    [SerializeField] private Button rollDiceButton;

    private int activePlayerIndex = 0;
    private bool isTurnInProgress = false;

    private const int ACTIVE_PRIORITY = 20;
    private const int INACTIVE_PRIORITY = 10;

    private void Start()
    {
        // 1. Initial setup: focus camera on Player 1 in Default View
        SetCameraTargets(players[activePlayerIndex]);
        SetDefaultCameraActive();

        // 2. Hook up dice roll button listener
        rollDiceButton.onClick.AddListener(OnDiceButtonClicked);
    }

    // Called when player clicks the Roll button
    public void OnDiceButtonClicked()
    {
        if (isTurnInProgress) return;

        StartCoroutine(ExecuteTurnSequence());
    }

    private IEnumerator ExecuteTurnSequence()
    {
        isTurnInProgress = true;
        rollDiceButton.interactable = false;

        // STEP 1: Switch to Zoom View as movement/action begins
        SetZoomCameraActive();

        // Wait a brief moment for camera to zoom in smoothly
        yield return new WaitForSeconds(0.8f);

        // STEP 2: Roll dice & move player
        int rolledNumber = Random.Range(1, 7); // Replace with your dice rolling logic
        Debug.Log($"Player {activePlayerIndex + 1} rolled: {rolledNumber}");

        // Move the player piece step-by-step
        yield return StartCoroutine(MovePlayerAlongTiles(players[activePlayerIndex], rolledNumber));

        // STEP 3: Finish movement, small pause to let player see final landing spot
        yield return new WaitForSeconds(1.0f);

        // STEP 4: Switch turn to the next player
        AdvanceToNextPlayer();

        // Re-enable dice button for next player
        rollDiceButton.interactable = true;
        isTurnInProgress = false;
    }

    private void AdvanceToNextPlayer()
    {
        // Loop index: 0 -> 1 -> 2 -> 3 -> 0
        activePlayerIndex = (activePlayerIndex + 1) % players.Count;

        // Set both cameras to target the new player
        SetCameraTargets(players[activePlayerIndex]);

        // Return camera to default view for the new player
        SetDefaultCameraActive();
    }

    private void SetCameraTargets(Transform target)
    {
        defaultCamera.Follow = target;
        defaultCamera.LookAt = target;

        zoomCamera.Follow = target;
        zoomCamera.LookAt = target;
    }

    private void SetDefaultCameraActive()
    {
        defaultCamera.Priority = ACTIVE_PRIORITY;
        zoomCamera.Priority = INACTIVE_PRIORITY;
    }

    private void SetZoomCameraActive()
    {
        zoomCamera.Priority = ACTIVE_PRIORITY;
        defaultCamera.Priority = INACTIVE_PRIORITY;
    }

    // Replace this simulation with your actual board tile movement logic
    private IEnumerator MovePlayerAlongTiles(Transform player, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            // Simple hop/move forward simulation
            Vector3 startPos = player.position;
            Vector3 endPos = player.position + (player.forward * 2.0f);

            float elapsed = 0f;
            float stepDuration = 0.4f;

            while (elapsed < stepDuration)
            {
                player.position = Vector3.Lerp(startPos, endPos, elapsed / stepDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            player.position = endPos;
            yield return new WaitForSeconds(0.1f);
        }
    }
}