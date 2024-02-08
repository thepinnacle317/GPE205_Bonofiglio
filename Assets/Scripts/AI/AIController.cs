using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIController : Controller
{
    public enum AIState { Idle, Chase, Attack, Defend, Flee, ReturntoPost, Scan}
    public AIState currentState;
    private float lastStateTimeChange;
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
                // transition
                break;

                // Do this for each state.
        }
    }

    public void Scan()
    {
        // Execute scan logic here.
    }

    public virtual void ChangeState(AIState newState)
    {
        currentState = newState;
        lastStateTimeChange = Time.fixedTime;
    }
}
