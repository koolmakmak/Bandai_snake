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

    [Header("Visual Snakes & Ladders")]
    public bool drawSnakesLadders = true;
    public float connectorHeight = 0.5f;
    public float ladderWidth = 0.3f;
    public float snakeWidth = 0.25f;
    public Color ladderColor = new Color(0.25f, 0.8f, 0.35f);
    public Color snakeColor = new Color(0.85f, 0.2f, 0.2f);

    [System.NonSerialized] List<GameObject> generatedVisuals = new List<GameObject>();

    // งูและบันไดสำหรับกระดาน 55 ช่อง — แก้ตัวเลขให้ตรงกับภาพงู/บันไดบนกระดานได้
    static readonly TeleportNode[] DefaultTeleports = new TeleportNode[]
    {
        // บันได (ขึ้น)
        new TeleportNode { fromTile = 2,  toTile = 23 },
        new TeleportNode { fromTile = 7,  toTile = 27 },
        new TeleportNode { fromTile = 12, toTile = 34 },
        new TeleportNode { fromTile = 18, toTile = 41 },
        new TeleportNode { fromTile = 31, toTile = 50 },
        new TeleportNode { fromTile = 39, toTile = 53 },
        // งู (ลง)
        new TeleportNode { fromTile = 48, toTile = 14 },
        new TeleportNode { fromTile = 42, toTile = 20 },
        new TeleportNode { fromTile = 35, toTile = 11 },
        new TeleportNode { fromTile = 26, toTile = 6 },
        new TeleportNode { fromTile = 52, toTile = 30 },
        new TeleportNode { fromTile = 46, toTile = 17 },
    };

    void Awake()
    {
        // สคริปต์จะดึงตำแหน่งลูกๆ ทั้ง 100 แผ่นเรียงตามลำดับใน Hierarchy มาให้อัตโนมัติ!
        waypoints = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }

        // ถ้ายังไม่ได้ตั้งใน Inspector ให้ใช้ค่างู/บันไดมาตรฐาน
        if (teleports == null || teleports.Count == 0)
            teleports = new List<TeleportNode>(DefaultTeleports);

        if (drawSnakesLadders)
            DrawTeleports();
    }

    [ContextMenu("Fill Default Teleports")]
    void FillDefaultTeleports()
    {
        teleports = new List<TeleportNode>(DefaultTeleports);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Draw Snakes & Ladders")]
    public void DrawTeleports()
    {
        ClearTeleportVisuals();

        if (waypoints == null || teleports == null) return;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        foreach (var node in teleports)
        {
            if (node.fromTile < 1 || node.toTile < 1 || node.fromTile > waypoints.Length || node.toTile > waypoints.Length)
                continue;

            Vector3 from = waypoints[node.fromTile - 1].position + Vector3.up * connectorHeight;
            Vector3 to = waypoints[node.toTile - 1].position + Vector3.up * connectorHeight;
            bool ladder = node.toTile > node.fromTile;

            GameObject go = new GameObject(ladder ? "Ladder" : "Snake");
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(transform);
            generatedVisuals.Add(go);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            if (shader != null) lr.material = new Material(shader);
            lr.startColor = ladder ? ladderColor : snakeColor;
            lr.endColor = lr.startColor;
            lr.startWidth = ladder ? ladderWidth : snakeWidth;
            lr.endWidth = lr.startWidth;
            lr.numCapVertices = 8;
            lr.numCornerVertices = 8;

            if (ladder)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, from);
                lr.SetPosition(1, to);
            }
            else
            {
                int segments = 32;
                lr.positionCount = segments + 1;
                Vector3 dir = to - from;
                Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
                if (side.sqrMagnitude < 0.0001f) side = Vector3.right;

                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 p = Vector3.Lerp(from, to, t);
                    p += side * (Mathf.Sin(t * Mathf.PI * 3f) * dir.magnitude * 0.08f);
                    lr.SetPosition(i, p);
                }
            }
        }
    }

    [ContextMenu("Clear Snakes & Ladders")]
    public void ClearTeleportVisuals()
    {
        for (int i = generatedVisuals.Count - 1; i >= 0; i--)
        {
            if (generatedVisuals[i] != null)
            {
                if (Application.isPlaying) Destroy(generatedVisuals[i]);
                else DestroyImmediate(generatedVisuals[i]);
            }
        }
        generatedVisuals.Clear();
    }
}