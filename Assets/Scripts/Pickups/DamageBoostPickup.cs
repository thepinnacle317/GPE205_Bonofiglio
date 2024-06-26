using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class DamageBoostPickup : PickupBase
{
    public DamageBoostPowerup damagePowerup;
    public AudioClip boostClip;
    public void OnTriggerEnter(Collider other)
    {
        PowerupManager powerupManager = other.GetComponent<PowerupManager>();

        // Check if there is a valid Powerup Manager
        if (powerupManager != null)
        {
            // Add the Health Powerup
            powerupManager.Add(damagePowerup);

            AudioSource.PlayClipAtPoint(boostClip, transform.position);

            // Destroy the Health Pickup
            Destroy(gameObject);
        }
    }
}
