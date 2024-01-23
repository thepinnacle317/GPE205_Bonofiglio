using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateClockwiseKey;
    public KeyCode rotateCounterClockwiseKey;

    public override void Start()
    {
        // Super
        base.Start();
    }

    public override void Update()
    {
        // Process keyboard inputs
        ProcessInputs();

        // Super
        base.Update();
    }

    public override void ProcessInputs()
    {
        if (Input.GetKey(moveForwardKey))
        {
            pawn.MoveForward();
        }
        if (Input.GetKey(moveBackwardKey)) 
        {
            pawn.MoveBackward();
        }
        if (Input.GetKey(rotateClockwiseKey)) 
        {
            pawn.RotateClockwise();
        }
        if (Input.GetKey(rotateCounterClockwiseKey)) 
        {
            pawn.RotateCounterClockwise();
        }

    }
}
