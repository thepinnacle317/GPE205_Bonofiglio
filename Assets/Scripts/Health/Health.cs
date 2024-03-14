using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Health : MonoBehaviour
{
    /* Health Variables */
    public float currentHealth;
    public float maxHealth = 100f;

    /* Armor Variables */
    public float currentArmor;
    public float maxArmor = 150f;

    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = 0f;
    }

    void Update()
    {

    }

    public void TakeDamage(float damage, Pawn source)
    {
        // Subtract damage amount from the players armor if they have any
        if (currentArmor > 0f) 
        {
            currentArmor -= damage;
        }
        // Otherwise take health damage
        else
        {
            currentHealth -= damage;
        }
        // Debug Log
        Debug.Log(source.name + " did " + damage + " damage to " + gameObject.name);

        if (currentHealth <= 0f)
        {
            Die(source);
        }
    }

    public void Heal(float healAmount, Pawn target)
    {
        if (healAmount > 0f)
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

    public void AddArmor(float armorAmount,  Pawn target)
    {
        if (armorAmount != maxArmor) 
        {
            currentArmor += armorAmount;
        }
        currentArmor = Mathf.Clamp(currentArmor, 0, maxArmor);
        Debug.Log(target.name + " gained " + armorAmount + ". " + " Current armor is now: " + currentArmor);
    }

    public void RemoveArmor(float armorAmount, Pawn target)
    {
        if (armorAmount > 0f)
        {
            currentArmor -= armorAmount;
            currentArmor = Mathf.Clamp(currentArmor, 0, maxArmor);
            Debug.Log(target.name + " had " + armorAmount + " removed. " + " Current armor is now: " + currentArmor);
        }
    }
}
