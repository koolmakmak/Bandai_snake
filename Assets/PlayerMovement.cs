using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator anim;
    private CharacterController controller;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if keyboard is connected
        if (Keyboard.current == null) return;

        // 1. Read input from WASD or Arrow keys using the new system
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. Check if moving
        bool isMoving = direction.magnitude >= 0.1f;

        // 3. Update Animator
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
        }

        // 4. Move and rotate
        if (isMoving)
        {
            transform.rotation = Quaternion.LookRotation(direction);

            if (controller != null)
            {
                controller.Move(direction * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}