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

    public bool hasSightOfPlayer = false;
    private Vector3 closestSightLine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        closestSightLine = transform.position;
    }

    public void Move(Transform target)
    {
        if (target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPosition = target.position;
        hasSightOfPlayer = HasLineOfSight(target, currentPos);

        if (!hasSightOfPlayer && (transform.position - closestSightLine).magnitude < .5f)
        {
            closestSightLine = FindNearestLoSPosition(target);
            targetPosition = closestSightLine;
        }
        else if (!hasSightOfPlayer)
        {
            if (!HasLineOfSight(transform, closestSightLine))
            {
                closestSightLine = FindNearestLoSPosition(target);
            }
            targetPosition = closestSightLine;
        }
        else closestSightLine = transform.position;

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);

        CheckAccelerating(distance);

        if (hasSightOfPlayer && distance <= retreatDistance)
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
    }

    private bool HasLineOfSight(Transform startTransform, Vector2 target)
    {
        Vector2 start = startTransform.position;
        Vector2 direction = (target - start).normalized;
        float distance = Vector2.Distance(start, target);

        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            if (hit.collider.CompareTag("Environment"))
            {
                Debug.DrawRay(start, direction * distance, Color.red, 0.1f);
                return false;
            }
        }
        Debug.DrawRay(start, direction * distance, Color.green, 0.1f);

        return true;
    }

    private Vector2 FindNearestLoSPosition(Transform target)
    {
        float stepSize = 1f;
        Queue<Vector2> queue = new Queue<Vector2>();
        HashSet<Vector2> visited = new HashSet<Vector2>();

        Vector2 start = transform.position;
        Vector2 bestFailure = transform.position;
        Vector2 targetPos = new Vector2(target.position.x, target.position.y);
        queue.Enqueue(start);
        visited.Add(RoundVector(start));

        int maxIterations = 50;
        int iterations = 0;

        while (queue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            Vector2 current = queue.Dequeue();

            if (HasLineOfSight(target, current))
            {
                return current;
            }

            Vector2[] directions = new Vector2[]
            {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right
            };

            foreach (Vector2 d in directions)
            {
                Vector2 neighbor = current + d * stepSize;
                Vector2 roundedNeighbor = RoundVector(neighbor);
                if (!visited.Contains(roundedNeighbor) && HasLineOfSight(transform, neighbor))
                {
                    visited.Add(roundedNeighbor);
                    queue.Enqueue(neighbor);

                    if ((neighbor - targetPos).magnitude < (bestFailure - targetPos).magnitude)
                    {
                        bestFailure = neighbor;
                    }
                }
            }
        }
        return bestFailure;
    }

    private Vector2 RoundVector(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x * 1000f) / 1000f, Mathf.Round(v.y * 1000f) / 1000f);
    }
}

