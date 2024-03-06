using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Powerup
{
    // Powerup effect duration
    public float duration;
    // Check on how the powerup is applied
    public bool isPermanent;

   public abstract void Apply(PowerupManager target);
   public abstract void Remove(PowerupManager target);
}
