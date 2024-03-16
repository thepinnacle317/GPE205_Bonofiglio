using FXV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmorPowerup : Powerup
{
    public float armorToAdd;

    public override void Apply(PowerupManager target)
    {
        // Give Armor to the entity that collects this powerup
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.AddArmor(armorToAdd, target.GetComponent<TankPawn>());
            Debug.Log("Powerup Added");
        }
    }

    public override void Remove(PowerupManager target)
    {
        // Remove the armor from the player
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            // Remove all the armor the player has
            targetHealth.RemoveArmor(targetHealth.currentArmor, target.GetComponent<Pawn>());
            Debug.Log("Powerup Removed");
        }
    }
}
