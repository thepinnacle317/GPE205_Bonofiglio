using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBoostPickup : PickupBase
{
    public DamageBoostPowerup damagePowerup;
    public void OnTriggerEnter(Collider other)
    {
        PowerupManager powerupManager = other.GetComponent<PowerupManager>();

        // Check if there is a valid Powerup Manager
        if (powerupManager != null)
        {
            // Add the Health Powerup
            powerupManager.Add(damagePowerup);

            // Destroy the Health Pickup
            Destroy(gameObject);
        }
    }
}
