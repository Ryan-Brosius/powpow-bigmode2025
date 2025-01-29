using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovementStats MoveStats;

    private Rigidbody2D rb;

    private Vector2 moveDir = Vector2.zero;
    private Vector2 moveVelocity = Vector2.zero;
    private bool isFacingRight = true;

    private Controls playerControls;

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Awake()
    {
        playerControls = new Controls();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>();
        Move(MoveStats.Acceleration, MoveStats.Deceleration, moveDir);
    }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {

        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = moveInput * MoveStats.MaxWalkSpeed;

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.velocity = moveVelocity;
        }
        else
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.velocity = moveVelocity;
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }

        if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
}
