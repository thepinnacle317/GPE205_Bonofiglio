using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{

    public List<Powerup> powerups;
    public List<Powerup> removedPowerupQueue;

    void Start()
    {
        powerups = new List<Powerup>();
    }

    void Update()
    {
        DecrementPowerupTimers();
    }

    // LateUpdate is called at the end of a frame
    void LateUpdate()
    {
        ApplyRemovePowerupsQueue();       
    }

    public void Add(Powerup powerupToAdd)
    {
        // Apply this powerup
        powerupToAdd.Apply(this);

        // Add the powerup to the list
        powerups.Add(powerupToAdd);
    }

    public void Remove(Powerup powerupToRemove)
    {
        if (powerupToRemove.isPermanent == false)
        {
            // Remove the powerup
            powerupToRemove.Remove(this);

            // Get ready to remove the powerup
            removedPowerupQueue.Add(powerupToRemove);
        }
    }
    
    public void DecrementPowerupTimers()
    {
        // Search through the list of powerups
        foreach (Powerup powerup in powerups)
        {
            // Subtract frame draw time from duration
            powerup.duration -= Time.deltaTime;

            // Powerup duration has ended
            if (powerup.duration <= 0)
                Remove(powerup);
        }
    }

    private void ApplyRemovePowerupsQueue()
    {
        // Remove the powerups that are in the temporary list.
        foreach (Powerup powerup in removedPowerupQueue)
        {
            if (powerup == null) return;
            powerups.Remove(powerup);
        }
        // Reset the powerup queue list
        removedPowerupQueue.Clear();
    }
}
