using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardMovement : MonoBehaviour
{
    [Header("Waypoints / Path")]
    [Tooltip("Drag all board tiles/waypoints in order here")]
    public Transform[] waypoints;
    public int currentWaypointIndex = 0;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("State")]
    public bool isMoving = false;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Place character at the first waypoint if available
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[currentWaypointIndex].position;
        }
    }

    /// <summary>
    /// Call this function from your Dice script when the dice roll finishes.
    /// Example: player.MoveSteps(diceResult);
    /// </summary>
    public void MoveSteps(int steps)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(steps));
        }
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;
        if (anim != null) anim.SetBool("isWalking", true);

        for (int i = 0; i < steps; i++)
        {
            // Advance to next waypoint (loops around if reaching the end)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 targetPos = waypoints[currentWaypointIndex].position;

            // Move towards the target waypoint
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                // Smooth rotation towards the target tile
                Vector3 direction = (targetPos - transform.position).normalized;
                direction.y = 0; // Keep horizontal

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }

                // Move position
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Snap exactly to position
            transform.position = targetPos;
            yield return new WaitForSeconds(0.1f); // Brief pause at each tile
        }

        // Stop walking animation
        isMoving = false;
        if (anim != null) anim.SetBool("isWalking", false);
    }
}