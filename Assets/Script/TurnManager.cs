using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class TurnManager : MonoBehaviour
{
    private enum TurnState { Idle, Rolling, Moving, Switching }

    [Header("Cinemachine Cameras")]
    public CinemachineCamera vcamDefault;
    public CinemachineCamera vcamZoom;
    public CinemachineCamera vcamDice;
    public CinemachineCamera vcamBoard; // optional: static camera framing the whole board
    public Transform diceTarget;

    [Header("Game References")]
    public DiceRoller diceRoller;
    public List<PlayerController> players = new List<PlayerController>();
    public Button rollButton;

    [Header("Settings")]
    public float diceResultDelay = 2f;
    public float landingDelay = 1.2f;
    public int defaultPriority = 10;
    public int zoomPriority = 20;
    public int dicePriority = 30;
    public int boardPriority = 40;

    private enum CameraView { Default, Zoom, Dice }

    private TurnState state = TurnState.Idle;
    private int currentPlayerIndex = 0;
    private CinemachineBrain brain;

    private CameraView currentView = CameraView.Default;
    private bool boardViewActive = false;
    private CameraView viewBeforeBoard = CameraView.Default;

    private void Start()
    {
        if (diceTarget == null && diceRoller != null && diceRoller.drop_point != null)
            diceTarget = diceRoller.drop_point.transform;

        if (vcamDice != null && diceTarget != null)
        {
            vcamDice.Follow = diceTarget;
            vcamDice.LookAt = diceTarget;
        }

        if (players.Count > 0 && players[0] != null)
            PointPlayerCamerasAt(players[0].transform);

        if (brain == null && Camera.main != null)
            brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = FindAnyObjectByType<CinemachineBrain>();

        ShowDefault();
        SetRollButtonInteractable(true);
    }

    private void OnEnable()
    {
        if (diceRoller != null)
        {
            diceRoller.OnRollStart += HandleRollStart;
            diceRoller.OnDiceLanded += HandleDiceLanded;
        }
    }

    private void OnDisable()
    {
        if (diceRoller != null)
        {
            diceRoller.OnRollStart -= HandleRollStart;
            diceRoller.OnDiceLanded -= HandleDiceLanded;
        }
    }

    /// <summary>Wire this to the Roll Dice button's onClick event.</summary>
    public void OnRollClicked()
    {
        if (diceRoller != null)
            diceRoller.RollDice();
    }

    private void HandleRollStart()
    {
        if (state != TurnState.Idle) return;

        boardViewActive = false;
        state = TurnState.Rolling;
        SetRollButtonInteractable(false);
        ShowDice();
    }

    private void HandleDiceLanded(int steps)
    {
        if (state != TurnState.Rolling || players.Count == 0) return;

        state = TurnState.Moving;
        StartCoroutine(MoveAfterDiceDelay(steps));
    }

    private IEnumerator MoveAfterDiceDelay(int steps)
    {
        // Keep the dice camera on the result for a moment before following the player.
        yield return new WaitForSeconds(diceResultDelay);

        ShowZoom();

        // Wait for the blend into the zoom camera to finish before moving.
        yield return null;
        if (brain != null)
        {
            float timeLimit = Mathf.Max(brain.DefaultBlend.Time, 0.05f);
            float elapsed = 0f;
            while (brain.IsBlending && elapsed < timeLimit + 0.25f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        PlayerController player = players[currentPlayerIndex];
        if (player == null)
        {
            FinishTurn();
            yield break;
        }

        player.OnMovementComplete.AddListener(HandleMovementComplete);
        player.MoveSteps(steps);
    }

    private void HandleMovementComplete()
    {
        if (players.Count > 0)
        {
            PlayerController player = players[currentPlayerIndex];
            if (player != null)
                player.OnMovementComplete.RemoveListener(HandleMovementComplete);
        }

        FinishTurn();
    }

    private void FinishTurn()
    {
        if (state == TurnState.Switching) return;
        state = TurnState.Switching;
        StartCoroutine(SwitchTurnRoutine());
    }

    private IEnumerator SwitchTurnRoutine()
    {
        yield return new WaitForSeconds(landingDelay);

        if (players.Count > 0)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            if (players[currentPlayerIndex] != null)
                PointPlayerCamerasAt(players[currentPlayerIndex].transform);
        }

        boardViewActive = false;
        ShowDefault();
        SetRollButtonInteractable(true);
        state = TurnState.Idle;
    }

    private void PointPlayerCamerasAt(Transform target)
    {
        if (vcamDefault != null)
        {
            vcamDefault.Follow = target;
            vcamDefault.LookAt = target;
        }
        if (vcamZoom != null)
        {
            vcamZoom.Follow = target;
            vcamZoom.LookAt = target;
        }
    }

    private void ShowDefault()
    {
        currentView = CameraView.Default;
        SetCameraPriority(vcamDefault, defaultPriority);
        SetCameraPriority(vcamZoom, 0);
        SetCameraPriority(vcamDice, 0);
        SetCameraPriority(vcamBoard, 0);
    }

    private void ShowZoom()
    {
        currentView = CameraView.Zoom;
        SetCameraPriority(vcamDefault, 0);
        SetCameraPriority(vcamZoom, zoomPriority);
        SetCameraPriority(vcamDice, 0);
        SetCameraPriority(vcamBoard, 0);
    }

    private void ShowDice()
    {
        currentView = CameraView.Dice;
        SetCameraPriority(vcamDefault, 0);
        SetCameraPriority(vcamZoom, 0);
        SetCameraPriority(vcamDice, dicePriority);
        SetCameraPriority(vcamBoard, 0);
    }

    /// <summary>Wire this to the Board button's onClick event.</summary>
    public void OnBoardButtonClicked()
    {
        if (boardViewActive)
        {
            boardViewActive = false;
            RestoreView(viewBeforeBoard);
        }
        else
        {
            viewBeforeBoard = currentView;
            boardViewActive = true;
            ShowBoard();
        }
    }

    private void ShowBoard()
    {
        CinemachineCamera board = vcamBoard != null ? vcamBoard : vcamDefault;
        SetCameraPriority(vcamDefault, 0);
        SetCameraPriority(vcamZoom, 0);
        SetCameraPriority(vcamDice, 0);
        SetCameraPriority(vcamBoard, 0);
        SetCameraPriority(board, boardPriority);
    }

    private void RestoreView(CameraView view)
    {
        switch (view)
        {
            case CameraView.Zoom: ShowZoom(); break;
            case CameraView.Dice: ShowDice(); break;
            default: ShowDefault(); break;
        }
    }

    private void SetCameraPriority(CinemachineCamera cam, int priority)
    {
        if (cam != null)
            cam.Priority = priority;
    }

    private void SetRollButtonInteractable(bool value)
    {
        if (rollButton != null)
            rollButton.interactable = value;
    }
}
