using FXV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    public bool isAIPawn;
    
    public RaycastHit hitResults;

    public override void Start()
    {
        base.Start();
        shooter.shotDelay = fireRate;
    }

    public override void Update()
    {
        base.Update();

        // Check if the tank pawn is an AI.  If so, use the raycast to find the distance to an object so that they can use obstacle avoidance.
        if (isAIPawn == true)
        {
            LineTrace();
            // Player Pawns are set on the Ignore Raycast Layer so the the AI will not try to steer around them
            if (hitResults.distance < 50f)
            {
                // Rotate the AI Pawn 90 degrees left or right
                transform.Rotate(0, 90 * Time.deltaTime, 0);
                // Possibly add a check by getting the hit normal and then picking a side to favor to rotate towards.
            }
            if (hitResults.distance > 100f && hitResults.distance < 300)
            {
                // Rotate the ai pawn 5 degrees.
                transform.Rotate(0, 5 * Time.deltaTime, 0 );
            }
            
        }
    }

    public override void MoveForward()
    {
        mover.Move(transform.forward, moveSpeed);
    }

    public override void MoveBackward() 
    {
        mover.Move(transform.forward, -moveSpeed);
    }

    public override void RotateClockwise()
    {
        mover.Rotate(turnSpeed);
    }

    public override void RotateCounterClockwise()
    {
        mover.Rotate(-turnSpeed);
    }

    public override void Shoot()
    {
        shooter.Shoot(shellPrefab, shellForce, damageDone, shellLifespan);
    }

    public override void MakeNoise()
    {
        if (noiseMaker != null)
        {
            noiseMaker.volumeDistance = noiseMakerVolume;
        }
    }

    public override void StopNoise()
    {
        if (noiseMaker != null)
        {
            noiseMaker.volumeDistance = 0;
        }
    }

    /* AI Methods */
    public override void RotateTowards(Vector3 targetPosition)
    {
        // Get the vector from the enemy pawn to the target
        Vector3 vectorToTarget = targetPosition - transform.position;
        // Retrieve the look rotation
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);
        // Rotate to the vector at  the given speed for that frame
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    protected void LineTrace()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        
        Physics.Raycast(ray, out hitResults);
        Debug.DrawRay(transform.position, transform.forward);
    }
}
