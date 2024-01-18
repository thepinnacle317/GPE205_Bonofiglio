using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void MoveForward()
    {
        Debug.Log("Move Forwards");
    }

    public override void MoveBackward() 
    {
        Debug.Log("Move Backwards");
    }

    public override void RotateClockwise()
    {
        Debug.Log("Rotates Clockwise");
    }

    public override void RotateCounterClockwise()
    {
        Debug.Log("Rotates Counter Clockwise");
    }
}
