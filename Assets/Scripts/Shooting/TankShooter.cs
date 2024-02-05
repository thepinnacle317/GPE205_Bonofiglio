using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooter : Shooter
{
    // Transform to spawn tank shells from 
    public Transform firepointTransform;
    private float timeUntilNextShot;

    public override void Start()
    {
        timeUntilNextShot = shotDelay;
    }

    public override void Update()
    {
        timeUntilNextShot -= Time.deltaTime;
        if (timeUntilNextShot <= 0)
        {
            Debug.Log("Next Shot Ready");
            timeUntilNextShot = shotDelay;
        }
    }

    public override void Shoot(GameObject tankhellPrefab, float shellForce, float damageDone, float lifespan)
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
                damageOH.Owner = GetComponent<Pawn>();
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
    }
}
