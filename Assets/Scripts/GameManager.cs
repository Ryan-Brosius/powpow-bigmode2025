using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float slowMotionDuration = 0.05f;
    [SerializeField] private float slowMotionTimeScale = 0.25f;
    [SerializeField] private Material whiteMat;
    [HideInInspector] public Material WhiteMat => whiteMat;
    [SerializeField] private GameObject capturedHut;
    [HideInInspector] public GameObject CapturedHut => capturedHut;
    private float baseTimeScale = 1f;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private void Awake()
    {
        InitializeSingleton();

        baseTimeScale = Time.timeScale;
        DOTween.SetTweensCapacity(500, 50);
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
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

    Coroutine Slowmotion;
    public void StartSlowMotionEffect(float duration = -1f)
    {
        //if (gameOver) return;

        if (Slowmotion != null)
        {
            StopCoroutine(Slowmotion);
            Time.timeScale = baseTimeScale;
        }
        Slowmotion = StartCoroutine(SlowMotionEffect(duration < 0 ? slowMotionDuration : duration));
    }

    private IEnumerator SlowMotionEffect(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = slowMotionTimeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = baseTimeScale;
    }
}
