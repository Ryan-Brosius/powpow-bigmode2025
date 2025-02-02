using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovementStats MoveStats;
    [SerializeField] PlayerHealth playerHealthScript;

    private Rigidbody2D rb;

    private Vector2 moveDir = Vector2.zero;
    private Vector2 moveVelocity = Vector2.zero;
    private bool isFacingRight = true;

    private Controls playerControls;

    private static PlayerMovement instance;
    public static PlayerMovement Instance => instance;

    [System.Serializable]
    public class V2Event : UnityEvent<Vector2> { }
    [SerializeField] private V2Event changeSpot = new V2Event();
    public V2Event ChangeSpot => changeSpot;
    Vector2Int cachedSpot;

    [Header("Roll Controls")]
    [SerializeField] RollCase rollUI;
    [SerializeField] float rollSpeed = 15f;
    [SerializeField] float rollDuration = 0.2f;
    [SerializeField] float rollCooldown = 5f;
    public int maxRollCharges = 1;
    public int currentRollCharges = 1;
    bool isRolling = false;
    float rollTimer = 0f;


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
        playerHealthScript = GetComponent<PlayerHealth>();

        if (GameObject.Find("Roll Case")) rollUI = GameObject.Find("Roll Case").GetComponent<RollCase>();
        if (rollUI) rollUI.InitializeRollUI(maxRollCharges);
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (currentRollCharges >= 1)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) && !isRolling)
            {
                Roll();
            }
        }
    }

    private void FixedUpdate()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>();
        if (!isRolling) Move(MoveStats.Acceleration, MoveStats.Deceleration, moveDir);

        

        if (currentRollCharges < maxRollCharges)
        {
            rollTimer += Time.deltaTime;
            if (rollTimer > rollCooldown)
            {
                currentRollCharges++;
                if (rollUI) rollUI.UpdateCurrentRolls(currentRollCharges);
                rollTimer = 0f;
            }
        }
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

        var currentPos = transform.position;
        if (Vector2Int.RoundToInt((Vector2)currentPos) != cachedSpot)
        {
            cachedSpot = Vector2Int.RoundToInt((Vector2)currentPos);
            changeSpot.Invoke((Vector2)currentPos);
        }
    }

    private void Roll()
    {
        if (playerHealthScript) playerHealthScript.StartCoroutine("ImmunityFrame");

        currentRollCharges--;
        if (rollUI) rollUI.UpdateCurrentRolls(currentRollCharges);

        isRolling = true;

        rb.velocity = moveDir * rollSpeed;

        Invoke("EndRoll", rollDuration);
    }

    private void EndRoll()
    {
        isRolling = false;
    }

    public void IncreaseMaxRoll()
    {
        maxRollCharges++;
        currentRollCharges++;
        if (rollUI) rollUI.UpdateMaxRolls(maxRollCharges);
        if (rollUI) rollUI.UpdateCurrentRolls(currentRollCharges);
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
