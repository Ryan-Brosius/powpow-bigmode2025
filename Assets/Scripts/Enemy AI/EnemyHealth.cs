using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 10;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            //bad code sorry, fix later
            var defaultMat = transform.GetChild(0).GetComponent<SpriteRenderer>().material;
            transform.GetChild(0).GetComponent<SpriteRenderer>().material = GameManager.Instance.WhiteMat;
            GameManager.Instance.StartSlowMotionEffect();
            var seq = DOTween.Sequence();
            seq.Append(transform.GetChild(0).transform.DOShakePosition(0.5f, 0.5f));
            seq.AppendCallback(() => transform.GetChild(0).GetComponent<SpriteRenderer>().material = defaultMat);
        }
    }

    private void Die()
    {
        var deadEnemy = new GameObject("Dead Enemy");
        deadEnemy.transform.position = transform.position;
        deadEnemy.AddComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        deadEnemy.GetComponent<SpriteRenderer>().material = GameManager.Instance.WhiteMat;
        deadEnemy.transform.DOScale(Vector2.zero, 0.5f);

        Destroy(deadEnemy, 1f);
        GameManager.Instance.StartSlowMotionEffect();

        Destroy(gameObject);
    }
}

