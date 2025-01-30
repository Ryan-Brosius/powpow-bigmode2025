using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private static CameraController instance;
    public static CameraController Instance => instance;
    //private bool movedCamera = false;

    private Transform playerTransform;

    private void Awake()
    {
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

    void Start()
    {
        playerTransform = PlayerMovement.Instance.transform;
    }

    void Update()
    {
        transform.position = playerTransform.position;
    }

    public void ShakeCamera()
    {
        transform.DOShakePosition(0.1f, 0.15f, 6).OnComplete(() =>
        {
            transform.position = Vector2.zero;
        });
    }
}
