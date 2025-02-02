using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100; //Change later
    [SerializeField] private int currentHealth;
    HealthCase healthUI;
    [SerializeField] private float iFrameDuration = 0.3f;
    [SerializeField] bool isImmune = false;

    private void Start()
    {
        // currentHealth = maxHealth;
        if (GameObject.Find("Health Case")) healthUI = GameObject.Find("Health Case").GetComponent<HealthCase>();

        if (healthUI) healthUI.InitializeHealthUI(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isImmune) return;

        currentHealth -= damage;
        StartCoroutine("ImmunityFrame");
        if (currentHealth >= maxHealth) currentHealth = maxHealth;
        if (healthUI) healthUI.UpdateHealthUI(damage);

        if (currentHealth <= 0)
        {
            Die();  
        }
    }

    public IEnumerator ImmunityFrame()
    {
        isImmune = true;
        Color playerSprite = this.GetComponent<SpriteRenderer>().color;
        playerSprite.a = 0.5f;
        this.GetComponent<SpriteRenderer>().color = playerSprite;
        yield return new WaitForSeconds(iFrameDuration);
        isImmune = false;
        playerSprite.a = 1f;
        this.GetComponent<SpriteRenderer>().color = playerSprite;
    }

    private void Die()
    {
        Destroy(gameObject);
        //Do something here later
    }
}
