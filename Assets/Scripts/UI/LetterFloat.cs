using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LetterFloat : MonoBehaviour
{
    [SerializeField] float floatHeight = 1f;
    [SerializeField] float floatSpeed = 1f;
    public bool makeFloat = true;

    public Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (makeFloat)
        {
            float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPosition.x, startPosition.y + newY, startPosition.z);
        }
    }


}
