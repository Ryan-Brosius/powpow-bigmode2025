using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScore;
    [SerializeField] SceneLoad sceneManagerScript;
    [SerializeField] ScoreManager scoreManagerScript;


    private void Awake()
    {
        sceneManagerScript = GameObject.Find("SceneManager").GetComponent<SceneLoad>();
        scoreManagerScript = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
    }
    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.anyKeyDown) if (sceneManagerScript)
            {
                Time.timeScale = 1f;
                sceneManagerScript.ReloadCurrentScene();
            }
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        if (scoreManagerScript) finalScore.text = scoreManagerScript.highScore.ToString();
    }
}
