using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]

public class LandminePickup : PickupBase
{
    public DamagePowerup damagePowerup;
    public AudioClip explosionSound;

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
            powerupManager.Add(damagePowerup);

            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            // Destroy the Health Pickup
            Destroy(gameObject);
        }
    }
}
