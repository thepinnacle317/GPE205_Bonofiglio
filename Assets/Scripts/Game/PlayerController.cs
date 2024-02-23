using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class PlayerController : Controller
{
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateClockwiseKey;
    public KeyCode rotateCounterClockwiseKey;
    public KeyCode shootKey;

    public override void Start()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            if (GameManager.instance.players != null)
            {
                // Add the controller to the players list in the GameManager
                GameManager.instance.players.Add(this);
            }
        }

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

    public void OnDestroy()
    {
        // Check to make sure there is a valid GameManager
        if (GameManager.instance != null)
        {
            if (GameManager.instance.players != null)
            {
                // Remove the player from the GameManagers player list.
                GameManager.instance.players.Remove(this);
            }
        }
    }

    public override void ProcessInputs()
    {
        if (Input.GetKey(moveForwardKey))
        {
            pawn.MoveForward();
            // Make Noise
            pawn.MakeNoise();
        }
        if (Input.GetKey(moveBackwardKey)) 
        {
            pawn.MoveBackward();
            // Make Noise
            pawn.MakeNoise();
        }
        if (Input.GetKey(rotateClockwiseKey)) 
        {
            pawn.RotateClockwise();
            // Make Noise
            pawn.MakeNoise();
        }
        if (Input.GetKey(rotateCounterClockwiseKey)) 
        {
            pawn.RotateCounterClockwise();
            // Make Noise
            pawn.MakeNoise();
        }
        if (Input.GetKeyDown(shootKey))
        {
            pawn.Shoot();
            // Make Noise
            pawn.MakeNoise();
        }
        if (!Input.GetKey(moveForwardKey) && !Input.GetKey(moveBackwardKey) && !Input.GetKey(rotateClockwiseKey) && 
            !Input.GetKey(rotateCounterClockwiseKey) && !Input.GetKeyDown(shootKey))
        {
            pawn.StopNoise();
        }
    }
}
