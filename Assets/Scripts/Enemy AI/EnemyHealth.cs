using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 10;
    [HideInInspector] public int Health
    {
        get { return health; }
        set { health = value; }
    }
    private bool decor;
    private GameObject ps;
    private List<EnemyHealth> connectedHuts = new();
    private Material defaultMat;

    [HideInInspector] public GameObject LetterToSpawn;

    [SerializeField] private int pointsOnDeath = 0;
    [SerializeField] private string hurtSound;
    [SerializeField] private string deathSound;

    private void Start()
    {
        if (transform.childCount > 0)
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
        connectedHuts.Clear();
        foreach (var connectedHut in connectedHuts2)
        {
            if (connectedHut != null && connectedHut != this)
            {
                connectedHuts.Add(connectedHut);
            }
        }
    }

    private void CleanupDestroyedConnections()
    {
        connectedHuts = connectedHuts.Where(hut => hut != null && hut.gameObject != null).ToList();
    }

    private void FlashWhite()
    {
        if (transform != null && transform.childCount > 0)
        {
            var childRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.material = GameManager.Instance.WhiteMat;
                GameManager.Instance.StartSlowMotionEffect();
                var seq = DOTween.Sequence();
                seq.Append(transform.GetChild(0).transform.DOShakePosition(0.25f, 0.5f));
                seq.AppendCallback(() =>
                {
                    if (childRenderer != null && defaultMat != null)
                    {
                        childRenderer.material = defaultMat;
                    }
                });
            }
        }
    }

    public void TakeDamage(int damage) => TakeDamage(true, damage);

    public void TakeDamage(bool conn, int damage)
    {
        CleanupDestroyedConnections();
        health -= damage;
        FlashWhite();

        if (conn)
        {
            foreach (var connectedHut in connectedHuts.ToList())
            {
                if (connectedHut != null && connectedHut.gameObject != null)
                {
                    connectedHut.TakeDamage(false, damage);
                }
            }
        }

        if (health <= 0)
        {
            if (deathSound != null)
            {
                SoundManager.Instance.PlaySoundEffect(deathSound);
            }

            Die();
            foreach (var connectedHut in connectedHuts.ToList())
            {
                if (connectedHut != null && connectedHut.gameObject != null)
                {
                    connectedHut.Die();
                }
            }
        } else
        {
            if (hurtSound != null)
            {
                SoundManager.Instance.PlaySoundEffect(hurtSound);
            }
        }
    }

    private void Die()
    {
        if (name.Contains("Hut"))
        {
            //this is embarrasing
            Instantiate(GameManager.Instance.CapturedHut, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        if (decor)
        {
            if (ps != null)
            {
                var particles = Instantiate(ps, transform.position, Quaternion.identity);
                Destroy(particles, 1f);
            }
        }
        else
        {
            var deadEnemy = new GameObject("Dead Enemy");
            deadEnemy.transform.position = transform.position;

            if (transform.childCount > 0)
            {
                var childSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
                if (childSprite != null)
                {
                    var deadRenderer = deadEnemy.AddComponent<SpriteRenderer>();
                    deadRenderer.sprite = childSprite.sprite;
                    deadRenderer.material = GameManager.Instance.WhiteMat;
                    deadEnemy.transform.DOScale(Vector2.zero, 0.5f);
                    Destroy(deadEnemy, 1f);
                }
            }
        }

        ScoreManager.Instance.AddScore(pointsOnDeath);

        GameManager.Instance.StartSlowMotionEffect();

        foreach (var hut in connectedHuts.ToList())
        {
            if (hut != null)
            {
                hut.connectedHuts.Remove(this);
            }
        }
        if (LetterToSpawn != null) Instantiate(LetterToSpawn, transform.position, Quaternion.identity);
        connectedHuts.Clear();

        Destroy(gameObject);
    }
}