using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("อ้างอิงกระดาน")]
    public board boardManager;

    public event System.Action<int> Win;

    [Header("ตั้งค่าการเคลื่อนที่")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    public int currentTileIndex = 0;

    // เพิ่ม Offset เพื่อปรับระยะความสูง/ความตรงของตัวละครบนกระดาน
    public Vector3 positionOffset = new Vector3(0, 0.5f, 0);

    [Header("Events")]
    public UnityEvent OnMovementComplete;
    public UnityEvent OnWin;

    private bool isMoving = false;
    private Animator anim; // 1. Animator reference

    void Start()
    {
        anim = GetComponentInChildren<Animator>(); // 2. Get Animator component
        if (anim != null) anim.applyRootMotion = false; // script moves the pawn, not the animation

        // ย้ายตัวละครไปช่องแรก + เพิ่ม Offset
        if (boardManager != null && boardManager.waypoints != null && boardManager.waypoints.Length > 0)
        {
            transform.position = boardManager.waypoints[0].position + positionOffset;
        }
    }

    public void MoveSteps(int steps)
    {
        if (!isMoving && boardManager != null)
        {
            StartCoroutine(MoveRoutine(steps));
        }
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        // 3. Start walking animation
        if (anim != null) anim.SetBool("isMoving", true);

        for (int i = 0; i < steps; i++)
        {
            if (currentTileIndex + 1 < boardManager.waypoints.Length)
            {
                currentTileIndex++;

                // บวก Offset เข้าไปเพื่อให้ยืนอยู่บนแผ่นพอดี
                Vector3 targetPosition = boardManager.waypoints[currentTileIndex].position + positionOffset;

                while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                    // 4. หมุนตัวเฉพาะแกน Y อย่างนุ่มนวล (ไม่ snap)
                    Vector3 lookTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
                    Vector3 direction = lookTarget - transform.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.0001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }

                    yield return null;
                }

                transform.position = targetPosition; // Snap ให้ตรงช่อง
                yield return new WaitForSeconds(0.1f); // พักสั้นๆ แต่ละช่อง
            }
            else
            {
                break; // ถึงช่องสุดท้ายแล้ว
            }
        }

        // 5. Stop walking animation (กลับไป Idle)
        if (anim != null) anim.SetBool("isMoving", false);

        CheckTeleport();
        isMoving = false;
        if (currentTileIndex==54)
        {
            OnWin.Invoke();
        }

        OnMovementComplete.Invoke();
    }

    private void CheckTeleport()
    {
        if (boardManager == null || boardManager.teleports == null) return;

        int currentBoardTile = currentTileIndex + 1;

        foreach (var node in boardManager.teleports)
        {
            if (node.fromTile == currentBoardTile)
            {
                currentTileIndex = node.toTile - 1;

                // บวก Offset ตอนวาร์ปด้วย
                transform.position = boardManager.waypoints[currentTileIndex].position + positionOffset;
                Debug.Log($"ตกช่องวาร์ป! จากช่อง {node.fromTile} ไปยัง {node.toTile}");
                break;
            }
        }
    }
}