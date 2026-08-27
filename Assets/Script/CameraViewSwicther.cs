using UnityEngine;
using DG.Tweening;

public class CameraViewSwitcher : MonoBehaviour
{
    public Transform mainCamera;
    public Transform boardCameraTarget;
    public float transitionDuration = 1.0f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isAtBoard = false;

    private void Start()
    {
        if(mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        if (mainCamera != null)
        {
            originalPosition = mainCamera.position;
            originalRotation = mainCamera.rotation;
        }
    }
    public void MoveCameraToBoard()
    {
        if (mainCamera == null || boardCameraTarget == null) return;

        mainCamera.DOKill();

        if (!isAtBoard)
        {
            mainCamera.DOMove(boardCameraTarget.position, transitionDuration);
            mainCamera.DORotate(boardCameraTarget.eulerAngles, transitionDuration);
            isAtBoard = true;
        }
        else
        {
            mainCamera.DOMove(originalPosition, transitionDuration);
            mainCamera.DORotate(originalRotation.eulerAngles, transitionDuration);
            isAtBoard = false;
        }
    }
}
