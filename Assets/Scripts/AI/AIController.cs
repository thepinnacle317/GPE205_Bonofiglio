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
    public enum AIState { Idle, Chase, Attack, Defend, Flee, ReturntoPost, RangedAttack, Patrolling }
    public AIState currentState;
    private float lastStateTimeChange;

    /* Targets */
    public GameObject target;
    public Controller targetController;

    /* Fleeing */
    public float fleeDistance;

    /* Hearing */
    public float hearingDistance;

    /* Sight */
    public float fieldOfView;

    /* Patrolling */
    public PatrolPoint[] patrolPoints;

    public enum PatrolType { Looping, Random, SinglePass, None }
    public PatrolType currentPatrolType;
    public float waypointsStopDistance;
    protected int currentWaypoint = 0;

    /* Defend */
    public float defendTime;
    protected float timeInDefense;

    /* Components */
    public Transform guardPostTransform;
    protected Health healthComp;

    public override void Start()
    {
        // Set the time that will be ticked against to the designer variable
        timeInDefense = defendTime;

        healthComp = pawn.GetComponent<Health>();

        if (GameManager.instance != null )
        {
            if (GameManager.instance.aiControllers != null)
            {
                GameManager.instance.aiControllers.Add(this);
            }
        }

        TargetPlayerOne();
        base.Start();
    }

    public override void Update()
    {
        // Used to handle the AI decision making.
        if (pawn != null)
        {
            ProcessInputs();
        }

        // This is broken.  *** Still trying to find the right solution
        if (currentState == AIState.Defend)
        {
            timeInDefense -= Time.time;
        }

        
        base.Update();
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
                if (healthComp.currentHealth <= 50)
                {
                    ChangeState(AIState.Flee);
                }
                break;

            case AIState.Flee:
                // Flee to safe position
                DoFleeState();
                ChangeState(AIState.ReturntoPost);
                break;

            case AIState.ReturntoPost:
                // Return to spawn location and Idle
                DoReturnToPostState();
                break;

            case AIState.Patrolling:
                // Exectute Patrol state
                DoPatrolState(PatrolType.Looping);
                if (target != null && IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Attack);
                }
                if (healthComp.currentHealth <= 50)
                {
                    ChangeState(AIState.Flee);
                }
                break;

            case AIState.Defend:
                Debug.Log("Defending");
                break;
        }
    }

    public virtual void ChangeState(AIState newState)
    {
        currentState = newState;
        lastStateTimeChange = Time.fixedTime;
    }

    protected void Idle()
    {
        // Do nothing
    }

    protected bool IsDistanceLessThan(GameObject target, float distance)
    {
        if (Vector3.Distance(pawn.transform.position, target.transform.position) < distance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /* Chase Methods */
    protected virtual void DoChaseState()
    {
        if (target != null)
        {
            Chase(target);
        }
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

    /* Attack Methods */
    public virtual void DoAttackState()
    {
        // Chase the player
        if (target != null)
        {
            Chase(target);
        }
        // Shoot at the player
        Shoot();
    }

    public void Shoot()
    {
        pawn.Shoot();
    }

    /* Flee Methods */
    public virtual void DoFleeState()
    {
        Flee();
        pawn.moveSpeed = 6;
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

    /* Patrol Methods */
    public virtual void DoPatrolState(PatrolType type)
    {
        if (currentPatrolType == PatrolType.SinglePass)
        {
            Patrol();
        }
        else if (currentPatrolType == PatrolType.Looping)
        {
            Patrol();
            if (currentWaypoint > patrolPoints.Length - 1)
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
        // If there are enough patrolPoints in the array to move to a waypoint.
        if (patrolPoints.Length > currentWaypoint)
        {
            // Move to the patrolPoint
            Chase(patrolPoints[currentWaypoint].transform);
            // If close enough to the destination patrolPoints, increment to the next patrolPoints to travel to.
            if (Vector3.Distance(pawn.transform.position, patrolPoints[currentWaypoint].transform.position) <= waypointsStopDistance)
            {
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
        // If there are enough patrolPoints in the array to move to a waypoint.
        if (patrolPoints.Length > currentWaypoint)
        {
            // Move to the patrolPoint
            Chase(patrolPoints[currentWaypoint].transform);
            // If close enough to the destination patrolPoints, increment to the next patrolPoints to travel to.
            if (Vector3.Distance(pawn.transform.position, patrolPoints[currentWaypoint].transform.position) < waypointsStopDistance)
            {
                // Choose a random patrolPoint
                currentWaypoint = Random.Range(0, patrolPoints.Length);
            }
        }
    }

    protected void ReturnToGuardPost()
    {
        if (guardPostTransform != null)
        {
            // Move to guard post location
            Chase(guardPostTransform.position);
            if (Vector3.Distance(pawn.transform.position, guardPostTransform.position) <= 5)
            {
                ChangeState(AIState.Defend);
            }
        }
    }

    protected void DoReturnToPostState()
    {
        ReturnToGuardPost();
    }

    protected void DoDefendState()
    {
        IsDefending();
    }

    protected void IsDefending()
    {
        // Make sure that there is a valid target and in defend time
        if ( target != null && timeInDefense > 0)
        {
            // Rotate toward the target and if the target is within engagement range then shoot them
            pawn.RotateTowards(target.transform.position);
            if (IsDistanceLessThan(target, 10))
            {
                Shoot();
            }
        }
    }

    protected bool IsHit()
    {
        // Check if the target was hit.
        return false;
    }

    /* Debug Helper Function */
    // TODO:Make a final version that will find the closest player and set them as the target.
    public void TargetPlayerOne()
    {
        // Check that there is a valid instance of the gamemanager
        if (GameManager.instance != null)
        {
            // Check that the array of players exists
            if (GameManager.instance.players != null)
            {
                // Check that there is a valid player
                if (GameManager.instance.players.Count > 0)
                {
                    // Target the gameObject of the pawn of the first player controller in the list
                    target = GameManager.instance.players[0].pawn.gameObject;
                }
            }
        }
    }

    protected bool HasTarget()
    {
        return (target != null);
    }

    protected bool CanHear(GameObject target)
    {
        // Get the target's NoiseMaker
        NoiseMaker noiseMaker = target.GetComponent<NoiseMaker>();
        if (noiseMaker == null)
        {
            return false;
        }
        // If no noise is being made or within the range
        if (noiseMaker.volumeDistance <= 0)
        {
            return false;
        }
        // The target is making noise, add the volumeDistance in the noisemaker to the hearingDistance of this AI
        float totalDistance = noiseMaker.volumeDistance + hearingDistance;
        // If the distance between our pawn and target is closer than totalDistance
        if (Vector3.Distance(pawn.transform.position, target.transform.position) <= totalDistance)
        {
            // Can hear the target
            return true;
        }
        else
        {
            // We are too far away from the target to hear them
            return false;
        }
    }

    protected bool CanSee(GameObject target)
    {
        Vector3 aiToTargetVector = target.transform.position - pawn.transform.position;

        float angleToTarget = Vector3.Angle(aiToTargetVector, pawn.transform.forward);

        if (angleToTarget < fieldOfView)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
