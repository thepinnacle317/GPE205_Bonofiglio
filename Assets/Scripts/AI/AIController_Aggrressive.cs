using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Aggrressive : AIController
{
    public override void Start()
    {
        currentState = AIState.Patrolling; 
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void ProcessInputs()
    {
        switch (currentState) 
        {
            case AIState.Patrolling:
                DoPatrolState(PatrolType.Random);
                if (CanSee(target) || CanHear(target))
                {
                    ChangeState(AIState.Chase);
                }
                break;

            case AIState.Chase:
                DoChaseState();
                if (target != null && IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Attack);
                }
                break;

            case AIState.Attack:
                DoAttackState();
                if (!IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Chase);
                }
                break;
        }
    }
}
