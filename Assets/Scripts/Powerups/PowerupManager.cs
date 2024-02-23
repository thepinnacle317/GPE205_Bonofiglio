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
        
    }

    // LateUpdate is called at the end of a frame
    void LateUpdate()
    {
        
    }

    public void Add(Powerup powerupToAdd)
    {
        // Add the powerup
    }

    public void Remove(Powerup powerupToRemove)
    {
        // Remove the powerup

        // Get ready to remove the powerup

    }
    /*
    public void DecrementPowerupTimers()
    {
        // Search through the list of powerups
        foreach ()
        {

        }
    }

    
    private void ApplyRemovePowerupsQueue()
    {
        foreach()
        {

        }
    }
    */
}
