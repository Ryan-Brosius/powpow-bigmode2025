using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunParent : MonoBehaviour
{
    public Vector2 Direction { get; set; }

    private Controls playerControls;
    private SpriteRenderer childSprite;

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
        childSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = mousePos - transform.parent.position;


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= 90 || angle < -90)
        {
            childSprite.flipY = true;
        } else
        {
            childSprite.flipY = false;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
