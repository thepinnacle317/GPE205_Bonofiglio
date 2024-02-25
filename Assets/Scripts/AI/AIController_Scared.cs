using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Scared : AIController
{ 
    public override void Start()
    {
        currentState = AIState.Patrolling;
        base.Start();
    }

    public override void Update()
    {
        Debug.Log(timeInDefense);
        Debug.Log(currentState);
        base.Update();
    }

    public override void ProcessInputs()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                DoPatrolState(PatrolType.Looping);
                if (CanHear(target) == true)
                {
                    ChangeState(AIState.Attack);
                }
                break;
            case AIState.Attack:
                DoAttackState();
                if (healthComp.currentHealth <= 50)
                {
                    ChangeState(AIState.Flee);
                }
                break;
            case AIState.Flee:
                DoFleeState();
                ChangeState(AIState.ReturntoPost);
                break;
            case AIState.ReturntoPost:
                DoReturnToPostState();
                break;
            case AIState.Defend:
                DoDefendState();
                if (timeInDefense <= 0)
                {
                    ChangeState(AIState.Patrolling);
                    timeInDefense = defendTime;
                }
                break;
        }
    }
}
