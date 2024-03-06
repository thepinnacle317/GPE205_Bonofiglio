using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : PickupBase
{
    public HealthPowerup healthPowerup;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public void OnTriggerEnter(Collider other)
    {
        PowerupManager powerupManager = other.GetComponent<PowerupManager>();

        // Check if there is a valid Powerup Manager
        if (powerupManager != null )
        {
            // Add the Health Powerup
            powerupManager.Add(healthPowerup);

            // Destroy the Health Pickup
            Destroy(gameObject);
        }
    }
}
