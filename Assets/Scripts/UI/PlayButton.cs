using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private DropZone playDropZone;
    [SerializeField] private SceneLoad sceneManager;
    [SerializeField] string nextScene;

    private void Awake()
    {
        playDropZone = this.GetComponent<DropZone>();
        sceneManager = GameObject.Find("Scene Manager").GetComponent<SceneLoad>();
    }

    private void Update()
    {
        if (playDropZone.currentCaseString == "POWER")
        {
            if (sceneManager) sceneManager.LoadScene(nextScene);
        }
    }
}
