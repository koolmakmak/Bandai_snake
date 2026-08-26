using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("อ้างอิงกระดาน")]
    public board boardManager;

    [Header("ตั้งค่าการเคลื่อนที่")]
    public float moveSpeed = 5f;
    public int currentTileIndex = 0;

    // เพิ่ม Offset เพื่อปรับระยะความสูง/ความตรงของตัวละครบนกระดาน
    public Vector3 positionOffset = new Vector3(0, 0.5f, 0);

    private bool isMoving = false;

    void Start()
    {
        // ย้ายตัวละครไปช่องแรก + เพิ่ม Offset
        if (boardManager != null && boardManager.waypoints.Length > 0)
        {
            transform.position = boardManager.waypoints[0].position + positionOffset;
        }
    }

    public void MoveSteps(int steps)
    {
        if (!isMoving) StartCoroutine(MoveRoutine(steps));
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            if (currentTileIndex + 1 < boardManager.waypoints.Length)
            {
                currentTileIndex++;

                // บวก Offset เข้าไปเพื่อให้ยืนอยู่บนแผ่นพอดี
                Vector3 targetPosition = boardManager.waypoints[currentTileIndex].position + positionOffset;

                transform.LookAt(targetPosition);

                while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }

        CheckTeleport();

        CheckIfFinish();

        isMoving = false;
    }

    private void CheckTeleport()
    {
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

    private void CheckIfFinish()
    {
        int lastblock = 
    }
}