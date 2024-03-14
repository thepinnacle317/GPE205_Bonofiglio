using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPickup : PickupBase
{
    public ArmorPowerup armorPowerup;

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        PowerupManager powerupManager = other.GetComponent<PowerupManager>();

        // Check if there is a valid Powerup Manager
        if (powerupManager != null)
        {
            // Add the Health Powerup
            powerupManager.Add(armorPowerup);

            // Destroy the Health Pickup
            Destroy(gameObject);
        }
    }
}
