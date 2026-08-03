using System.Collections.Generic;
using UnityEngine;

// โครงสร้างข้อมูลสำหรับเชื่อมจุดวาร์ป (งู และ บันได)
[System.Serializable]
public struct TeleportNode
{
    public int fromTile; // ช่องที่เหยียบ (เช่น 4 หรือ 98)
    public int toTile;   // ช่องที่จะถูกส่งไป (เช่น 14 หรือ 79)
}

public class board : MonoBehaviour
{
    [Header("ตำแหน่งช่องกระดานทั้งหมด")]
    public Transform[] waypoints;

    [Header("ตั้งค่างูและบันไดใน Inspector")]
    public List<TeleportNode> teleports;

    void Awake()
    {
        // สคริปต์จะดึงตำแหน่งลูกๆ ทั้ง 100 แผ่นเรียงตามลำดับใน Hierarchy มาให้อัตโนมัติ!
        waypoints = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }
}