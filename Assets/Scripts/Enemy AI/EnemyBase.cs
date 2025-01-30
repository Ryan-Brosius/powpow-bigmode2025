using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] EnemyAttack enemyAttack;
    [SerializeField] EnemyMovement enemyMovement;
    private Transform target;

    protected virtual void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(Attack());
    }

    protected virtual void Update()
    {
        enemyMovement.Move(target);
    }

    protected virtual IEnumerator Attack()
    {
        while (true)
        {
            if (enemyAttack != null)
            {
                yield return new WaitForSeconds(enemyAttack.fireRate);

                enemyAttack.PerformAttack(transform, target);
            }
        }
    }
}
