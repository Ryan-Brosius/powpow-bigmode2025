using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float speed = 10f;
    [SerializeField] public float timeUntilDeath = 5f;
    [SerializeField] public float deathTimer = 0.4f;
    [SerializeField] string doNotCollideTag = "Player";
    public int Damage { get; set; }
    public int Pierce { get; set; }

    private void Awake()
    {
        StartCoroutine(DeathTimer(timeUntilDeath));
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.right * Time.fixedDeltaTime * speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && !collision.gameObject.CompareTag(doNotCollideTag))
        {
            damageable.TakeDamage(Damage);

            if (--Pierce == 0 || collision.gameObject.CompareTag("Environment"))
            {
                StartCoroutine(BulletDeath());
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && !collision.gameObject.CompareTag(doNotCollideTag))
        {
            damageable.TakeDamage(Damage);

            if (--Pierce == 0 || collision.gameObject.CompareTag("Environment"))
            {
                StartCoroutine(BulletDeath());
            }
        }
    }

    public IEnumerator DeathTimer(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(BulletDeath());
    }

    IEnumerator BulletDeath()
    {
        speed = 0f;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(deathTimer);
        Destroy(gameObject);
    }
}
