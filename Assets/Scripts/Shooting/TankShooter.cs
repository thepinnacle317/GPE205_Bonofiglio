using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooter : Shooter
{
    // Transform to spawn tank shells from 
    public Transform firepointTransform;
    private float timeUntilNextShot;
    private bool bCanFire = false;

    public override void Start()
    {
        timeUntilNextShot = shotDelay;
    }

    public override void Update()
    {
        timeUntilNextShot -= Time.deltaTime;
        if (timeUntilNextShot <= 0)
        {
            // Set the bool that will allow the player to shoot 
            bCanFire = true;
            timeUntilNextShot = shotDelay;
        }
    }

    public override void Shoot(GameObject tankhellPrefab, float shellForce, float damageDone, float lifespan)
    {
        if (bCanFire == true)
        {
            // Instantiate the tank shell
            GameObject newShell = Instantiate(tankhellPrefab, firepointTransform.position, firepointTransform.rotation)
                as GameObject;

            // Get the DamageOnHit component
            DamageOnHit damageOH = newShell.GetComponent<DamageOnHit>();

            // Check to make sure the component exists
            if (damageOH != null)
            {
                // set the damageDone in the Hit component to the value passed in.
                damageOH.damageDone = damageDone;

                // Check for a valid owner of the Hit component
                damageOH.attacker = GetComponent<Pawn>();
            }

            // Retrieve Rigid Body component
            Rigidbody rb = newShell.GetComponent<Rigidbody>();

            // Check to make sure it has a valid RigidBody before running logic
            if (rb != null)
            {
                // Add force to make it move forward
                rb.AddForce(firepointTransform.forward * shellForce);
            }
            Destroy(newShell, lifespan);

            // If the player has fired then reset the bool so that the timer can reset it.
            bCanFire = false;
        }
    }
}
