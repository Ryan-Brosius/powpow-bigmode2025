using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    public char letterValue = 'P';
    [SerializeField] float floatHeight = 1f;
    [SerializeField] float floatSpeed = 1f;
    [SerializeField] float rotationSpeed = 45f;
    PowWordUIManager powWordUI;
    [SerializeField] string pickupSound = "LetterPickup";

    Vector3 startPosition;

    private void Awake()
    {
        powWordUI = GameObject.FindGameObjectWithTag("Bullets").GetComponent<PowWordUIManager>();
        startPosition = this.transform.position;
    }

    private void Update()
    {
        // Move the object up and down using Mathf.Sin
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + newY, startPosition.z);

        // Rotate the object around the Y-axis
        //transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (pickupSound != null)
        {
            SoundManager.Instance.PlaySoundEffect(pickupSound);
        }

        if (collision.tag == "Player")
        {
            if (letterValue == 'P' || letterValue == 'O' || letterValue == 'W')
            {
                if (powWordUI) powWordUI.LetterPickup(letterValue);
            }
            else if (letterValue == 'E')
            {
                collision.GetComponent<PlayerHealth>().TakeDamage(-1);
            }
            else if (letterValue == 'R')
            {
                collision.GetComponent<PlayerMovement>().IncreaseMaxRoll();
            }
            Destroy(gameObject);
        }
    }
}
