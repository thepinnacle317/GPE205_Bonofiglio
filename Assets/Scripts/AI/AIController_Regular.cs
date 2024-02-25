using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController_Regular : AIController
{
    
    public override void Start()
    {
        currentState = AIState.Idle;
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
            case AIState.Idle:
                
                break;
            case AIState.Patrolling:
                DoPatrolState(PatrolType.Random);
                if (CanSee(target))
                {
                    ChangeState(AIState.Chase);
                }
                break;
            case AIState.Chase:
                DoChaseState();
                if (IsDistanceLessThan(target, 10))
                {
                    ChangeState(AIState.Attack);
                }
                if (healthComp.currentHealth < 25)
                {
                    ChangeState(AIState.Patrolling);
                }
                break;
            case AIState.Attack:
                DoAttackState();
                if (healthComp.currentHealth < 25)
                {
                    ChangeState(AIState.Patrolling);
                }
                break;
        }
    }
}
