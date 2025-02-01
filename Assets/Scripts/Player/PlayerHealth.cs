using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100; //Change later
    [SerializeField] private int currentHealth;
    HealthCase healthUI;

    private void Start()
    {
        // currentHealth = maxHealth;
        healthUI = GameObject.Find("Health Case").GetComponent<HealthCase>();

        if (healthUI) healthUI.InitializeHealthUI(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth >= maxHealth) currentHealth = maxHealth;
        if (healthUI) healthUI.UpdateHealthUI(damage);

        if (currentHealth <= 0)
        {
            Die();  
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        //Do something here later
    }
}
