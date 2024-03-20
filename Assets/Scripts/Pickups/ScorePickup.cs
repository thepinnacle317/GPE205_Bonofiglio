using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePickup : PickupBase
{
    /* Score amount variable */
    public int scoreAmount;

    private Controller controller;

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        // Get the TankPawn comp so we can check a bool to see if it is an ai or not being used.
        TankPawn tankPawn = other.GetComponent<TankPawn>();
        controller = tankPawn.controller;
        // Do not let AI pickup score pickups.
        if (tankPawn.isAIPawn != true)
        {
            controller.AddToScore(scoreAmount);
            Debug.Log(tankPawn.name + " has " + controller.score);

            // Destroy the Score Pickup
            Destroy(gameObject);
        }
    }
}
