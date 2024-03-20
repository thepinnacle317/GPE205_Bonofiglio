using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamagePowerup : Powerup
{
    // Variable for damage amount to apply
    public int damageAmount;

    // Start is called before the first frame update
    public override void Apply(PowerupManager target)
    {
        // Apply damage to the entity that overlaps with this powerup
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null) 
        {
            targetHealth.TakeDamage(damageAmount, null,  target.GetComponent<Pawn>());
        }
    }

    public override void Remove(PowerupManager target)
    {
    }
}
