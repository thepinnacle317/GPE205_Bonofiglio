using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Panic : AIController
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
                if (healthComp.currentHealth <= 35)
                {
                    DoPatrolState(PatrolType.Random);
                }
                else
                {
                    DoPatrolState(PatrolType.SinglePass);
                }
               
                if (CanHear(target) || CanSee(target))
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
                if (healthComp.currentHealth <= 35)
                {
                    ChangeState(AIState.Flee);
                }
                break;
            case AIState.Attack:
                DoAttackState();
                if (!IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Chase);
                }
                if (healthComp.currentHealth <= 35)
                {
                    ChangeState(AIState.Flee);
                }
                break;
            case AIState.Flee:
                DoFleeState();
                ChangeState(AIState.Patrolling);
                break;
        }
    }
}
