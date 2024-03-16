using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageBoostPowerup : Powerup
{
    public float damageBoost;
    public override void Apply(PowerupManager target)
    {
       Pawn owningPawn = target.GetComponent<Pawn>();
        if (owningPawn != null)
        {
            owningPawn.damageDone += damageBoost;
            Debug.Log("Powerup Added");
        }
    }

    public override void Remove(PowerupManager target)
    {
        Pawn owningPawn = target.GetComponent<Pawn>();
        if (owningPawn != null)
        {
            owningPawn.damageDone -= damageBoost;
            Debug.Log("Powerup Removed");
        }
    }
}
