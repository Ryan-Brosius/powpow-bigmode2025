using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScoreOnDestroy : MonoBehaviour
{
    [SerializeField] private int points = 100;
    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            ScoreManager.Instance.AddScore(points);
        }
    }
}
