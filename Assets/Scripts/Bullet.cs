using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float timeUntilDeath = 5f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !other.gameObject.CompareTag(doNotCollideTag))
        {
            damageable.TakeDamage(Damage);
        }

        if (--Pierce == 0)
        {
            Destroy(gameObject);
        }
    }


    IEnumerator DeathTimer(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
