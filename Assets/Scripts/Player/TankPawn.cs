using FXV;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    public override void Start()
    {
        base.Start();
        shooter.shotDelay = fireRate;
    }

    public override void Update()
    {
        base.Update();
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

    public override void Ocillate(float angle)
    {
        // Have the enemy tank pawn rotate a set direction amount over time.
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

    public void ShieldVFXStatus()
    {
        GetComponent<Shield>().SetShieldActive(true);
    }
}
