using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float timeUntilDeath = 3f;
    [SerializeField] float deathTimer = 0.2f;
    [SerializeField] string doNotCollideTag = "Enemy";
    public int Damage = 1;
    public int Pierce = 1;

    private void Awake()
    {
        StartCoroutine(DeathTimer(timeUntilDeath));
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.right * Time.fixedDeltaTime * speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !other.gameObject.CompareTag(doNotCollideTag))
        {
            damageable.TakeDamage(Damage);

            if (--Pierce == 0)
            {
                StartCoroutine(BulletDeath());
            }
        }
    }


    IEnumerator DeathTimer(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(BulletDeath());
    }

    IEnumerator BulletDeath()
    {
        speed = 0f;

        if (gameObject.TryGetComponent<BoxCollider2D>(out var boxCollider))
        {
            boxCollider.enabled = false;
        }
        if (gameObject.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.enabled = false;
        }
        yield return new WaitForSeconds(deathTimer);
        Destroy(gameObject);
    }
}
