using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Following Settings")]
    [SerializeField] private float mouseInfluence = 0.15f;
    [SerializeField] private float smoothSpeed = 0.15f;
    [SerializeField] private float maxMouseDistance = 5f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.5f;
    [SerializeField] private int shakeVibrato = 12;

    private Transform playerTransform;
    private Camera mainCam;
    private Transform cameraTransform;
    private Vector2 screenSize;
    private Tweener currentShake;

    private static CameraController instance;
    public static CameraController Instance => instance;

    private void Awake()
    {
        InitializeSingleton();
        cameraTransform = transform.GetChild(0);
        mainCam = cameraTransform.GetComponent<Camera>();
        cameraTransform.localPosition = new Vector3(0, 0, -10);
        UpdateScreenSize();
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
        playerTransform = PlayerMovement.Instance.transform;
    }

    private void UpdateScreenSize()
    {
        screenSize.y = mainCam.orthographicSize * 2;
        screenSize.x = screenSize.y * mainCam.aspect;
    }

    private void LateUpdate()
    {
        if (!playerTransform) return;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector2 mouseScreenPos = mainCam.ScreenToViewportPoint(Input.mousePosition);

        Vector2 mouseOffset = new Vector2(
            (mouseScreenPos.x - 0.5f) * screenSize.x,
            (mouseScreenPos.y - 0.5f) * screenSize.y
        );

        mouseOffset = Vector2.ClampMagnitude(mouseOffset, maxMouseDistance);
        Vector3 newTargetPos = playerTransform.position + (Vector3)(mouseOffset * mouseInfluence);

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, newTargetPos + Vector3.forward * -10, smoothSpeed);
    }

    public void ShakeCamera(float strength = 0.5f)
    {
        if (currentShake != null && currentShake.IsPlaying())
        {
            currentShake.Kill();
        }
        currentShake = transform.DOShakePosition(shakeDuration, strength, shakeVibrato).SetUpdate(true);
    }

    public void QuickShake()
    {
        transform.DOShakePosition(0.1f, 0.5f, 6);
    }
}