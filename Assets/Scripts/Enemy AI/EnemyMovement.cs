using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Information")]
    [Tooltip("Maximum movement speed")]
    [SerializeField] private float maxSpeed = 3f;
    [Tooltip("Maximum acceleration")]
    [SerializeField] private float acceleration = 5f;
    [Tooltip("Maximum deceleration")]
    [SerializeField] private float deceleration = 5f;
    [Tooltip("Distance enemy stops approaching target")]
    [SerializeField] private float stoppingDistance = 1.5f;
    [Tooltip("Distance enemy moves away if target it too close")]
    [SerializeField] private float retreatDistance = 0.7f;
    [Tooltip("Radius to avoid other enemies")]
    [SerializeField] private float avoidanceRadius = 1.5f;
    [Tooltip("How strongly enemies avoid eachother")]
    [SerializeField] private float avoidanceStrength = 10f;
    [Tooltip("Smallest velocity an enemy can have")]
    [SerializeField] private float velocityClamp = 0.05f;

    private Rigidbody2D rb;
    private Vector2 velocity = Vector2.zero;
    private bool accelerating = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Transform target)
    {
        if (target == null) return;

        Vector2 targetPosition = target.position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);

        CheckAccelerating(distance);

        if (distance <= retreatDistance)
        {
            direction = -direction;
        }

        direction += AvoidOtherEnemies();
        direction = direction.normalized;

        if (accelerating)
        {
            velocity = Vector2.Lerp(velocity, direction * maxSpeed, Time.fixedDeltaTime * acceleration);
        } else
        {
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.fixedDeltaTime * deceleration);
        }


        if (velocity.magnitude > velocityClamp)
        {
            rb.velocity = velocity;
        } else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private Vector2 AvoidOtherEnemies()
    {
        Vector2 avoidanceForce = Vector2.zero;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject && enemy.CompareTag("Enemy"))
            {
                Vector2 awayFromEnemy = (Vector2)transform.position - (Vector2)enemy.transform.position;
                avoidanceForce += awayFromEnemy.normalized / awayFromEnemy.magnitude;
            }
        }

        return avoidanceForce * avoidanceStrength;
    }

    private void CheckAccelerating(float distance)
    {
        accelerating = distance <= retreatDistance || distance >= stoppingDistance;
        Debug.Log(accelerating);
    }
}

