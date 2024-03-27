using FXV;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;

public class Health : MonoBehaviour
{
    /* Health Variables */
    public float currentHealth;
    public float maxHealth = 100f;

    /* Armor Variables */
    public float currentArmor;
    public float maxArmor = 150f;

    public TankPawn tankPawn;

    private void Start()
    {
        currentHealth = maxHealth;
        currentArmor = 0f;
    }

    private void Update()
    {
        
    }

    public void TakeDamage(float damage, Pawn attacker, Pawn target)
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

            UpdateHealthUI(target);
        }
        // Print who took damage and how much
        Debug.Log(target.name + " took " + damage + " damage.");

        // If the pawn is out of health then call die.
        if (currentHealth <= 0f)
        {
            Die(target);
            attacker.controller.AddToScore(15);
        }
    }

    public void Heal(float healAmount, Pawn target)
    {
        if (healAmount > 0f)
        {
            currentHealth += healAmount;
            UpdateHealthUI(target);
        }
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(target.name + " healed for " + healAmount + ". " + " Current health is now: " + currentHealth);
    }

    public void Die(Pawn source)
    {
        if (source.controller.pawn.GetComponent<TankPawn>().isAIPawn != true)
        {
            Debug.Log("Player Pawn has Died");
            // Subtract a life when the player dies
            if (source.controller.currentLives > 0)
            {
                // Decrement the players lives
                source.controller.currentLives--;
                
                // Check to make sure there is a valid GameManager
                if (GameManager.instance != null)
                {
                    // Are there current players in the game
                    if (GameManager.instance.players != null)
                    {
                        Destroy(gameObject);
                        // Respawn the player if they have enough lives
                        GameManager.instance.RespawnPlayer();

                        // Update the lives in the player UI
                        UpdateLivesUI(source.controller);

                        // Print the players current lives
                        Debug.Log(source.controller.currentLives);
                    }
                }
            }
            if (source.controller.currentLives <= 0)
            {
                Destroy(gameObject);
                // Activate Game Over if the player is out of lives.
                GameManager.instance.ActivateGameOverScreen();

                // TODO: Make a check that both players are out of lives for split screen and solo play separately.
            }
        }
        
        Destroy(gameObject);
    }

    public void AddArmor(float armorAmount,  Pawn target)
    {
        if (armorAmount != maxArmor) 
        {
            currentArmor += armorAmount;
            currentArmor = Mathf.Clamp(currentArmor, 0, maxArmor);
            UpdateArmorUI(target);
            Debug.Log(target.name + " gained " + armorAmount + ". " + " Current armor is now: " + currentArmor);
        }
    }

    public void RemoveArmor(float armorAmount, Pawn target)
    {
        if (armorAmount > 0f)
        {
            currentArmor -= armorAmount;
            currentArmor = Mathf.Clamp(currentArmor, 0, maxArmor);
            UpdateArmorUI(target);
            Debug.Log(target.name + " had " + armorAmount + " removed. " + " Current armor is now: " + currentArmor);
        }
    }

    public void UpdateHealthUI(Pawn owningPawn)
    {
        Health healthComp = owningPawn.GetComponent<Health>();
        if (owningPawn.healthImage != null)
        {
            owningPawn.healthImage.fillAmount = currentHealth / maxHealth;
        }
    }

    public void UpdateArmorUI(Pawn owningPawn)
    {
        Health healthComp = owningPawn.GetComponent<Health>();
        if (owningPawn.healthImage != null)
        {
            owningPawn.armorImage.fillAmount = currentArmor / maxArmor;
        }
    }

    public void UpdateLivesUI(Controller owningController)
    {
        if (owningController.pawn.GetComponent<TankPawn>().isAIPawn == false)
        {
            Health healthComp = owningController.pawn.GetComponent<Health>();
            owningController.pawn.livesText.text = owningController.currentLives.ToString();
        }
    }
}
