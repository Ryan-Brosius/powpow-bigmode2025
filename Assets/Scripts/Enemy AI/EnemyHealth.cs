using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 10;
    private bool decor;
    private GameObject ps;

    private List<EnemyHealth> connectedHuts = new();

    private Material defaultMat;
    private void Start()
    {
        if(transform.childCount > 0)
            defaultMat = transform.GetChild(0).GetComponent<SpriteRenderer>().material;
    }

    public void AssignDecor(GameObject newPs)
    {
        defaultMat = GetComponent<SpriteRenderer>().material;
        decor = true;
        health = 1;
        ps = newPs;
    }

    public void AssignHut(List<EnemyHealth> connectedHuts2)
    {
        foreach(var connectedHut in connectedHuts2)
        {
            if (connectedHut != this)
            {
                connectedHuts.Add(connectedHut);
            }
        }
    }

    public void TakeDamage(int damage) => TakeDamage(true, damage);

    public void TakeDamage(bool conn, int damage)
    {
        health -= damage;

        if (conn)
        {
            foreach (var connectedHut in connectedHuts)
            {
                connectedHut.TakeDamage(false, damage);
            }
        }

        if (health <= 0)
        {
            Die();
        }
        else
        {
            //bad code sorry, fix later
            transform.GetChild(0).GetComponent<SpriteRenderer>().material = GameManager.Instance.WhiteMat;
            GameManager.Instance.StartSlowMotionEffect();


            var seq = DOTween.Sequence();
            seq.Append(transform.GetChild(0).transform.DOShakePosition(0.25f, 0.5f));
            seq.AppendCallback(() =>
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().material = defaultMat;
            });
        }
    }

    private void Die()
    {
        
        if (decor)
        {
            //deadEnemy.AddComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
            var particles = Instantiate(ps, transform.position, Quaternion.identity);
            Destroy(particles, 1f);
        }
        else
        {
            var deadEnemy = new GameObject("Dead Enemy");
            deadEnemy.transform.position = transform.position;
            deadEnemy.AddComponent<SpriteRenderer>().sprite = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            deadEnemy.GetComponent<SpriteRenderer>().material = GameManager.Instance.WhiteMat;
            deadEnemy.transform.DOScale(Vector2.zero, 0.5f);

            Destroy(deadEnemy, 1f);
        }
        GameManager.Instance.StartSlowMotionEffect();

        Destroy(gameObject);
    }
}

