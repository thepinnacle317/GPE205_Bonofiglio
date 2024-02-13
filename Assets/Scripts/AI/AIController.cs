using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AIController : Controller
{
    /* AI States */
    public enum AIState { Idle, Chase, Attack, Defend, Flee, ReturntoPost, Scan, RangedAttack, Patrolling}
    public AIState currentState;
    private float lastStateTimeChange;

    /* Targets */
    public GameObject target;
    public Controller targetController;

    /* Fleeing */
    public float fleeDistance;

    /* Patrolling */
    public Transform[] waypoints;
    public enum PatrolType { Looping, Random, SinglePass, None}
    public PatrolType currentPatrolType;
    public float waypointsStopDistance;
    private int currentWaypoint = 0;
    //public bool isLooping;

    public override void Start()
    {
        currentState = AIState.Patrolling;
        currentPatrolType = PatrolType.Random;
        base.Start();
    }

    
    public override void Update()
    {
        // Used to handle the AI decision making.
        ProcessInputs();
        base.Update();
        Debug.Log(currentWaypoint);
    }

    public override void ProcessInputs()
    {
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
                // Transition to Idle State
                if (!IsDistanceLessThan(target, 10))
                {
                   ChangeState(AIState.Idle);
                }
                // Transition to Attack State
                if (target != null && IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Attack);
                }
                break;

            case AIState.Attack:
                // Attack the player
                DoAttackState();
                break;

            case AIState.Flee:
                // Flee to safe position
                // Add a health condition for fleeing
                DoFleeState();
                if (!IsDistanceLessThan(target, fleeDistance))
                {
                    ChangeState(AIState.Idle);
                }
                break;

            case AIState.ReturntoPost:
                // Return to spawn location and Idle
                break;

            case AIState.Patrolling:
                // Exectute Patrol state
                DoPatrolState();
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
        // Move to the target's transform
        pawn.MoveForward();
    }

    public void Chase(Vector3 targetPosition)
    {
        // Rotate towards the target
        pawn.RotateTowards(targetPosition);
        // Move to the target
        pawn.MoveForward();
    }

    public void Chase(Transform targetTransform)
    {
        Chase(targetTransform.position);
    }

    // Overload for using the target pawn's transform.
    public void Chase(Pawn targetPawn)
    {
        // Rotate towards the target
        Chase(targetPawn.transform);
    }

    public virtual void DoAttackState()
    {
        // Chase the player
        Chase(target);
        // Shoot at the player
        Shoot();
    }

    public void Shoot()
    {
        pawn.Shoot();
    }

    public virtual void DoFleeState()
    {
        Flee();
    }

    protected virtual void Flee()
    {
        // Vector to target
        Vector3 vectorToTarget = target.transform.position - pawn.transform.position;
        // Get the inverse vector
        Vector3 vectorAwayFromTarget = -vectorToTarget;
        // Travel direction and distance
        Vector3 fleeVector = vectorAwayFromTarget.normalized * fleeDistance;        
        Chase(pawn.transform.position + fleeVector);
    }

    public virtual void DoPatrolState()
    {
        if (currentPatrolType == PatrolType.SinglePass)
        {
            Patrol();
        }
        else if (currentPatrolType == PatrolType.Looping)
        {
            Patrol();
            if (currentWaypoint > waypoints.Length - 1)
            {
                RestartPatrol();
            }
        }
        else if (currentPatrolType == PatrolType.Random)
        {
            RandomPatrol();
        }
        else
        {
            currentPatrolType = PatrolType.None;
        }
        
    }

    protected void Patrol()
    {
        // If there are enough waypoint in the list to move to a waypoint.
        if (waypoints.Length > currentWaypoint)
        {
            // Move to the waypoint
            Chase(waypoints[currentWaypoint]);
            // If close enough to the destination waypoint, increment to the next waypoint to travel to.
            if (Vector3.Distance(pawn.transform.position, waypoints[currentWaypoint].position) <= waypointsStopDistance)
            {
                Debug.Log("Next Waypoint");
                currentWaypoint++;
            }
        }
    }
    protected void RestartPatrol()
    {
        // Reset the waypoint index so that pawn patrols on a loop until interrupted
        currentWaypoint = 0;
    }

    protected void RandomPatrol()
    {
        // If there are enough waypoint in the list to move to a waypoint.
        if (waypoints.Length > currentWaypoint)
        {
            // Move to the waypoint
            Chase(waypoints[currentWaypoint]);
            // If close enough to the destination waypoint, increment to the next waypoint to travel to.
            if (Vector3.Distance(pawn.transform.position, waypoints[currentWaypoint].position) < waypointsStopDistance)
            {
                currentWaypoint = Random.Range(0 , waypoints.Length);
            }
        }
    }
}
