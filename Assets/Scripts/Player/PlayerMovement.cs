using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 65f;
    [SerializeField] private float deceleration = 45f;
    [SerializeField] private float velocityPower = 0.96f;
    [SerializeField] private float frictionAmount = 0.2f; 

    [Header("Roll Settings")]
    [SerializeField] private float rollSpeed = 25f;
    [SerializeField] private float rollDuration = 0.15f;
    [SerializeField] private float rollCooldown = 3f;
    [SerializeField] private AnimationCurve rollSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public int maxRollCharges = 2;
    public int currentRollCharges;

    [Header("References")]
    [SerializeField] private RollCase rollUI;
    [SerializeField] private PlayerHealth playerHealthScript;
    private Rigidbody2D rb;
    private Controls playerControls;
    private Vector2 moveDir;
    private bool isFacingRight = true;
    private bool isRolling;
    private float rollTimer;
    private float currentRollTime;
    private static PlayerMovement instance;
    public static PlayerMovement Instance => instance;
    private Vector2Int cachedSpot;
    [SerializeField] private UnityEvent<Vector2> changeSpot = new UnityEvent<Vector2>();
    public UnityEvent<Vector2> ChangeSpot => changeSpot;
    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #region Initialization
    private void Awake()
    {
        SetupComponents();
        InitializeRollSystem();
    }

    private void SetupComponents()
    {
        playerControls = new Controls();
        rb = GetComponent<Rigidbody2D>();
        playerHealthScript = GetComponent<PlayerHealth>();

        rb.drag = 0f;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void InitializeRollSystem()
    {
        currentRollCharges = maxRollCharges;
        if (GameObject.Find("Roll Case"))
        {
            rollUI = GameObject.Find("Roll Case").GetComponent<RollCase>();
            rollUI.InitializeRollUI(maxRollCharges);
        }
        InitializeSingleton();
    }
    #endregion

    #region Movement System
    private void FixedUpdate()
    {
        if (isRolling)
        {
            UpdateRoll();
            return;
        }

        moveDir = playerControls.Player.Move.ReadValue<Vector2>();
        Move(moveDir);
        UpdateRollCooldown();
        UpdatePositionTracking();
    }

    private void Move(Vector2 moveInput)
    {
        Vector2 targetSpeed = moveInput * maxSpeed;

        Vector2 speedDiff = targetSpeed - rb.velocity;

        float accelRate = moveInput.magnitude > 0.01f ? acceleration : deceleration;

        Vector2 movement = speedDiff * accelRate;
        rb.AddForce(movement);

        if (moveInput.magnitude < 0.01f)
        {
            float friction = Mathf.Min(rb.velocity.magnitude, frictionAmount);
            rb.AddForce(-rb.velocity.normalized * friction * rb.mass, ForceMode2D.Impulse);
        }

        rb.velocity = rb.velocity.magnitude > 0.01f ?
            rb.velocity * Mathf.Pow(velocityPower, Time.fixedDeltaTime) :
            Vector2.zero;

        if (moveInput.magnitude > 0.01f)
        {
            TurnCheck(moveInput);
        }
    }
    #endregion

    #region Roll System
    private void Update()
    {
        if (currentRollCharges >= 1 && Input.GetKeyDown(KeyCode.Mouse1) && !isRolling)
        {
            StartRoll();
        }
    }

    private void StartRoll()
    {
        if (playerHealthScript) playerHealthScript.StartCoroutine("ImmunityFrame");

        currentRollCharges--;
        if (rollUI) rollUI.UpdateCurrentRolls(currentRollCharges);

        isRolling = true;
        currentRollTime = 0f;

        Vector2 rollDir = moveDir.magnitude > 0.01f ? moveDir.normalized :
                         isFacingRight ? Vector2.right : Vector2.left;

        rb.velocity = rollDir * rollSpeed;
    }

    private void UpdateRoll()
    {
        currentRollTime += Time.fixedDeltaTime;
        float rollProgress = currentRollTime / rollDuration;

        if (rollProgress >= 1f)
        {
            isRolling = false;
            return;
        }

        rb.velocity = rb.velocity.normalized * (rollSpeed * rollSpeedCurve.Evaluate(rollProgress));
    }

    private void UpdateRollCooldown()
    {
        if (currentRollCharges < maxRollCharges)
        {
            rollTimer += Time.fixedDeltaTime;
            if (rollTimer >= rollCooldown)
            {
                currentRollCharges++;
                if (rollUI) rollUI.UpdateCurrentRolls(currentRollCharges);
                rollTimer = 0f;
            }
        }
    }

    public void IncreaseMaxRoll()
    {
        maxRollCharges++;
        currentRollCharges++;
        if (rollUI)
        {
            rollUI.UpdateMaxRolls(maxRollCharges);
            rollUI.UpdateCurrentRolls(currentRollCharges);
        }
    }
    #endregion

    #region Helper Functions
    private void UpdatePositionTracking()
    {
        Vector2Int currentPos = Vector2Int.RoundToInt(transform.position);
        if (currentPos != cachedSpot)
        {
            cachedSpot = currentPos;
            changeSpot.Invoke(transform.position);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if ((isFacingRight && moveInput.x < 0) || (!isFacingRight && moveInput.x > 0))
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void OnEnable() => playerControls.Enable();
    private void OnDisable() => playerControls.Disable();
    #endregion
}