using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AIController : Controller
{
    public enum AIState { Idle, Chase, Attack, Defend, Flee, ReturntoPost, Scan, RangedAttack}
    public AIState currentState;
    private float lastStateTimeChange;
    public GameObject target;
    public Controller targetController;

    public override void Start()
    {
        currentState = AIState.Idle;
        base.Start();
    }

    
    public override void Update()
    {
        // Used to handle the AI decision making.
        ProcessInputs();
        base.Update();
    }

    public override void ProcessInputs()
    {
        Debug.Log("Making Decisions!");
        switch (currentState)
        {
            case AIState.Idle:
                // Do Idle actions
                Idle();
                if (IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Chase);
                }
                break;
            case AIState.Scan:
                // Scan for the player
                Scan();
                break;
            case AIState.Chase:
                // Chase the player
                DoChaseState();
                // Transition
                if (!IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Idle);
                }
                break;
            case AIState.Attack:
                // Attack the player
                break;
            case AIState.Flee:
                // Flee to safe position
                break;
            case AIState.ReturntoPost:
                // Return to spawn location and Idle
                break;
        }
    }

    public virtual void ChangeState(AIState newState)
    {
        currentState = newState;
        lastStateTimeChange = Time.fixedTime;
    }

    public void Scan()
    {
        // Execute scan logic here. 
        // This will utilize the Ocillate Method.
    }

    protected void Idle()
    {
        // Do nothing
    }

    protected bool IsDistanceLessThan(GameObject target, float distance)
    {
        if (Vector3.Distance (pawn.transform.position, target.transform.position) < distance )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual void DoChaseState()
    {
        Chase(target);
    }
    public void Chase(GameObject target)
    {
        // Rotate the enemy pawn
        pawn.RotateTowards(target.transform.position);
        // Move the enemy pawn towards the target
        pawn.MoveForward();
    }

    // Overload for using the player's Controller to get it position to chase towards
    public void Chase(Controller targetController)
    {
        // Rotate towards the target
        pawn.RotateTowards(targetController.pawn.transform.position);
        // Move to the the target's transform
        pawn.MoveForward();
    }

    public void Chase(Transform targetTransform)
    {
        //Chase(targetTransform.position);
    }

    // Overload for using the target pawn's transform.
    public void Chase(Pawn targetPawn)
    {
        // Rotate towards the target
        Chase(targetPawn.transform);
    }
}
