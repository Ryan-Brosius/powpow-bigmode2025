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

        if (enemyAttack != null) StartCoroutine(Attack());
    }

    protected virtual void Update()
    {
        if (enemyMovement != null)
        {
            enemyMovement.Move(target);
        }
    }

    protected virtual IEnumerator Attack()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemyAttack.fireRate + UnityEngine.Random.Range(0, enemyAttack.fireRateVariation));

            enemyAttack.PerformAttack(transform, target);
        }
    }
}
