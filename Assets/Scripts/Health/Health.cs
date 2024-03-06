using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth = 100;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {

    }

    public void TakeDamage(float damage, Pawn source)
    {
        // Subtract damage amount from player health
        currentHealth -= damage;

        // Debug Log
        Debug.Log(source.name + " did " + damage + " damage to " + gameObject.name);

        if (currentHealth <= 0)
        {
            Die(source);
        }
    }

    public void Heal(float healAmount, Pawn target)
    {
        if (healAmount > 0)
        {
            currentHealth += healAmount;
        }
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log(target.name + " healed for " + healAmount + ". " + " Current health is now: " + currentHealth);
    }

    public void Die(Pawn source)
    {
        Destroy(gameObject);
        Debug.Log(source.name + " destroyed " + gameObject.name);
    }
}
